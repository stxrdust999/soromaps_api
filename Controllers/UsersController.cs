using Microsoft.AspNetCore.Mvc;
using Soromaps.Data;
using Soromaps.Models;
using Soromaps.DTO;

namespace Soromaps.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        // GET: api/users/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public IActionResult Create([FromBody] UserDTO dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(user);
        }

        // PUT: api/users/1
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] UserDTO dto)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            user.UserName  = dto.UserName;
            user.Email = dto.Email;
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            return Ok(user);
        }

        // DELETE: api/users/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
