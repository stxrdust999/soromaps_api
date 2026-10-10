using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.Models;
using static soromaps_api.DTOs.PlaceDto;

namespace soromaps_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        [EndpointSummary("Lista todos os lugares")]
        [ProducesResponseType(typeof(IEnumerable<PlaceResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PlaceResponseDto>>> GetAll()
        {
            var places = await context.Places
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => PlaceResponseDto.FromEntity(p))
                .ToListAsync();

            return Ok(places);
        }

        [HttpGet("{id:guid}")]
        [EndpointSummary("Busca um lugar pela ID")]
        [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlaceResponseDto>> GetById(Guid id)
        {
            var place = await context.Places
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlaceId == id);

            if (place is null)
                return NotFound();

            return Ok(PlaceResponseDto.FromEntity(place));
        }

        [HttpPost]
        [EndpointSummary("Cadastra um novo lugar")]
        [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<PlaceResponseDto>> Create(CreatePlaceDto request)
        {
            // NOTA: O AuthorId idealmente deve vir do Token JWT (User.Claims)
            // Aqui estamos simulando um ID vazio apenas para compilar de acordo com sua model
            var currentUserId = Guid.Empty;

            var place = new Place
            {
                CategoryId = request.CategoryId,
                AuthorId = currentUserId,
                PlaceName = request.PlaceName.Trim(),
                Lat = request.Lat,
                Lng = request.Lng,
                Neighborhood = request.Neighborhood.Trim(),
                About = request.About.Trim(),
                Description = request.Description.Trim(),
                HasWifi = request.HasWifi,
                PetFriendly = request.PetFriendly,
                Status = Status.pending, // Definido automaticamente pelo sistema
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Places.Add(place);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = place.PlaceId }, PlaceResponseDto.FromEntity(place));
        }

        [HttpPatch("{id:guid}")]
        [EndpointSummary("Atualiza parcialmente um lugar")]
        [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlaceResponseDto>> Update(Guid id, UpdatePlaceDto request)
        {
            var place = await context.Places.FindAsync(id);

            if (place is null)
                return NotFound();

            if (request.CategoryId.HasValue) place.CategoryId = request.CategoryId.Value;
            if (request.PlaceName is not null) place.PlaceName = request.PlaceName.Trim();
            if (request.Lat.HasValue) place.Lat = request.Lat.Value;
            if (request.Lng.HasValue) place.Lng = request.Lng.Value;
            if (request.Neighborhood is not null) place.Neighborhood = request.Neighborhood.Trim();
            if (request.About is not null) place.About = request.About.Trim();
            if (request.Description is not null) place.Description = request.Description.Trim();
            if (request.HasWifi.HasValue) place.HasWifi = request.HasWifi.Value;
            if (request.PetFriendly.HasValue) place.PetFriendly = request.PetFriendly.Value;
            if (request.Status.HasValue) place.Status = request.Status.Value;

            place.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return Ok(PlaceResponseDto.FromEntity(place));
        }

        [HttpDelete("{id:guid}")]
        [EndpointSummary("Remove um lugar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var place = await context.Places.FindAsync(id);
            if (place is null) return NotFound();

            context.Places.Remove(place);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
