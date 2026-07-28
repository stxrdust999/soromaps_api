using Microsoft.AspNetCore.Mvc;
using Soromaps.Data;
using Soromaps.DTO;

namespace Soromaps.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == dto.UserName);

            if (user == null)
            {
                return Unauthorized("Usuário não encontrado");
            }

            bool senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if (!senhaCorreta)
            {
                return Unauthorized("Senha incorreta");
            }

            return Ok(new { id = user.Id, userName = user.UserName, userEmail = user.Email });
        }
    }
}
