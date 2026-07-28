using Microsoft.AspNetCore.Mvc;
using Soromaps.Data;
using Soromaps.Models;
using Soromaps.DTO;

namespace Soromaps.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarkersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MarkersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/markers
        [HttpGet]
        public IActionResult GetAll()
        {
            var markers = _context.Markers.ToList();
            return Ok(markers);
        }

        // GET: api/markers/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var marker = _context.Markers.Find(id);

            if (marker == null)
                return NotFound();

            return Ok(marker);
        }

        // POST: api/markers
        [HttpPost]
        public IActionResult Create([FromBody] MarkerDTO dto)
        {
            var marker = new Marker
            {
                Nome = dto.Nome,
                Lat = dto.Lat,
                Lng = dto.Lng
            };

            _context.Markers.Add(marker);
            _context.SaveChanges();

            return Ok(marker);
        }

        // PUT: api/markers/1
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MarkerDTO dto)
        {
            var marker = _context.Markers.Find(id);
            if (marker == null)
            {
                return NotFound();
            }
            marker.Nome = dto.Nome;
            marker.Lat = dto.Lat;
            marker.Lng = dto.Lng;
            _context.SaveChanges();
            return Ok(marker);
        }

        // DELETE: api/markers/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var marker = _context.Markers.Find(id);

            if (marker == null)
                return NotFound();

            _context.Markers.Remove(marker);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
