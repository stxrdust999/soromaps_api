using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Data;
using soromaps_api.DTOs.Users;
using BCrypt.Net;
using soromaps_api.Models;
using Microsoft.AspNetCore.Authorization;

namespace soromaps_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        [EndpointSummary("Lista todos os usuários")]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            var users = await context.Users
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .Select(u => UserResponseDto.FromEntity(u))
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        [EndpointSummary("Busca um usuário pela ID")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
                return NotFound();

            return Ok(UserResponseDto.FromEntity(user));
        }

        [HttpPost]
        [AllowAnonymous]
        [EndpointSummary("Cadastra um novo usuário")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterUserDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var emailExists = await context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if (emailExists)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflito",
                    Detail = "Já existe uma conta cadastrada com esse e-mail",
                    Extensions = { ["code"] = "user.email_taken" }
                });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = "explorer"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                UserResponseDto.FromEntity(user)
            );
        }

        [HttpPatch("{id:guid}")]
        [EndpointSummary("Atualiza parcialmente o perfil do usuário")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponseDto>> Update(Guid id, UpdateUserDto request)
        {
            var user = await context.Users.FindAsync(id);

            if (user is null)
                return NotFound();

            if (request.Name is not null)
                user.Name = request.Name.Trim();

            if (request.AvatarUrl is not null)
                user.AvatarUrl = request.AvatarUrl;

            if (request.Biography is not null)
                user.Biography = request.Biography;

            if (request.Neighborhood is not null)
                user.Neighborhood = request.Neighborhood;

            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return Ok(UserResponseDto.FromEntity(user));
        }

        [HttpDelete("{id:guid}")]
        [EndpointSummary("Remove um usuário")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await context.Users.FindAsync(id);

            if (user is null)
                return NotFound();

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
