using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs.Auth;
using soromaps_api.DTOs.Users;
using soromaps_api.Models;
using soromaps_api.Services;
using System.Security.Claims;

namespace soromaps_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AppDbContext context, ITokenService tokenService, IConfiguration configuration) : ControllerBase
    {
        /**
         * Pre-computed cost-12 BCrypt hash used when an email is not found.
         * Ensures constant verification time (~100ms) to defeat account enumeration via timing attacks.
         */
        private readonly string DummyHash = configuration["Auth:DummyHash"]
            ?? "$2a$12$e80yvY6B7Q1J6W7B4L6b..7P3Z8N9M1K2L3O4P5Q6R7S8T9U0V1W2";

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("LoginRateLimit")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            /**
             * Normalizes email lookup to lowercase and executes BCrypt verification regardless of user existence.
             */
            var user = await context.Users.SingleOrDefaultAsync(
                user => user.Email.ToLower() == request.Email.ToLower()
            );

            var passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user?.PasswordHash ?? DummyHash
            );

            if (user == null || !passwordValid)
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Invalid email or password.",
                    Extensions = { ["code"] = "auth.invalid_credentials" }
                };

                return Unauthorized(problem);
            }

            /**
             * Issues the short-lived access JWT alongside an opaque 7-day refresh token.
             * Stores only the SHA-256 digest in the database to prevent plain token leakage.
             */
            var accessToken = tokenService.GenerateAccessToken(user);
            var rawRefreshToken = tokenService.GenerateRefreshToken();
            var tokenHash = tokenService.HashToken(rawRefreshToken);

            var session = new Session
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            context.Sessions.Add(session);
            await context.SaveChangesAsync();

            var userResponse = new UserResponseDto(
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.AvatarUrl,
                user.Biography,
                user.Neighborhood,
                user.CreatedAt
            );

            return Ok(new AuthResponseDto(
                accessToken,
                rawRefreshToken,
                900,
                userResponse
            ));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            /**
             * Locates the active session by the SHA-256 digest of the provided raw refresh token.
             * Eager-loads the associated user to populate claims in the replacement JWT.
             */
            var tokenHash = tokenService.HashToken(request.RefreshToken);

            var session = await context.Sessions
                .Include(session => session.User)
                .SingleOrDefaultAsync(session => session.TokenHash == tokenHash);

            /**
             * Rejects invalid, expired, or previously revoked sessions.
             */
            if (session == null || session.RevokedAt != null || session.ExpiresAt <= DateTime.UtcNow)
            {
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Invalid, expired or revoked refresh token.",
                    Extensions = { ["code"] = "auth.invalid_refresh_token" }
                };

                return Unauthorized(problem);
            }

            /**
             * Implements Refresh Token Rotation:
             * Revokes the current session immediately and issues a fresh token pair.
             * Prevents replay attacks if a refresh token was intercepted.
             */
            session.RevokedAt = DateTime.UtcNow;

            var newAccessToken = tokenService.GenerateAccessToken(session.User);
            var newRawRefreshToken = tokenService.GenerateRefreshToken();
            var newTokenHash = tokenService.HashToken(newRawRefreshToken);

            var newSession = new Session
            {
                UserId = session.User.Id,
                TokenHash = newTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            context.Sessions.Add(newSession);
            await context.SaveChangesAsync();

            var userResponse = new UserResponseDto(
                session.User.Id,
                session.User.Name,
                session.User.Email,
                session.User.Role,
                session.User.AvatarUrl,
                session.User.Biography,
                session.User.Neighborhood,
                session.User.CreatedAt
            );

            return Ok(new AuthResponseDto(
                newAccessToken,
                newRawRefreshToken,
                900,
                userResponse
            ));
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            /**
             * Revokes the specific refresh session presented by the authenticated client.
             * Operation is idempotent: non-existing or already-revoked tokens return 204 quietly.
             */
            var tokenHash = tokenService.HashToken(request.RefreshToken);
            var session = await context.Sessions
                .SingleOrDefaultAsync(session => session.TokenHash == tokenHash);

            if (session != null && session.RevokedAt == null)
            {
                session.RevokedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LogoutAll()
        {
            /**
             * Extracts the authenticated user's ID from the JWT 'sub' claim (mapped to NameIdentifier)
             * and revokes all active sessions across all devices simultaneously.
             */
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var activeSessions = await context.Sessions
                    .Where(session => session.UserId == userId && session.RevokedAt == null)
                    .ToListAsync();

                foreach (var session in activeSessions)
                {
                    session.RevokedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
