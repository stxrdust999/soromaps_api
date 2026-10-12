using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using soromaps_api.Models;
using soromaps_api.Options;

namespace soromaps_api.Services;

public sealed class TokenService(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : ITokenService
{
    private readonly JwtOptions options = jwtOptions.Value;

    public string GenerateAccessToken(User user)
    {
        /**
         * Retrieves the signing key from configuration.
         * Requires at least 256 bits of entropy for HMAC-SHA256 security guarantees.
         */
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        /**
         * Embeds identity and authorization claims into the token payload:
         * - Sub: Unique user UUID.
         * - Role: Used by downstream authorization policies (e.g. explorer vs admin).
         * - Jti: Random nonce to ensure token uniqueness across identical timestamps.
         */
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToLowerInvariant()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        /**
         * Builds the descriptor with a short 15-minute lifespan to limit the attack window
         * in case of token leakage, relying on refresh tokens for session renewal.
         */
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(options.AccessTokenLifetimeMinutes),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        /**
         * Generates 32 cryptographically secure random bytes (256 bits)
         * using the operating system's CSPRNG, avoiding pseudo-random predictability.
         */
        var randomNumber = new byte[32];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    public string HashToken(string token)
    {
        /**
         * Computes a deterministic SHA-256 digest in lowercase hexadecimal (64 chars).
         * Only the hash is stored in the database so that raw refresh tokens are never
         * exposed in data breaches or query logs.
         */
        var bytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
