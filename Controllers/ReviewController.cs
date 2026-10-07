using Microsoft.AspNetCore.Mvc;
using soromaps_api.Data;
using Microsoft.EntityFrameworkCore;
using soromaps_api.DTOs;
using soromaps_api.Models;

namespace soromaps_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController(AppDbContext context) : ControllerBase
    {
        [HttpGet("place/{placeId:guid}")]
        [EndpointSummary("Lista todas as avaliações de um lugar específico")]
        [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetByPlaceId(Guid placeId)
        {
            var reviews = await context.Reviews
                .AsNoTracking()
                .Where(r => r.PlaceId == placeId)
                .Select(r => ReviewResponseDto.FromEntity(r))
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost]
        [EndpointSummary("Cria uma avaliação para um lugar")]
        [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ReviewResponseDto>> Create(CreateReviewDto request)
        {
            var currentUserId = Guid.Empty; // Substituir pela extração do token

            var review = new Review
            {
                PlaceId = request.PlaceId,
                UserId = currentUserId,
                ReviewContent = request.ReviewContent.Trim(),
                QtdStars = request.QtdStars
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, ReviewResponseDto.FromEntity(review));
        }

        [HttpPatch("{id:guid}")]
        [EndpointSummary("Edita uma avaliação existente")]
        public async Task<ActionResult<ReviewResponseDto>> Update(Guid id, UpdateReviewDto request)
        {
            var review = await context.Reviews.FindAsync(id);
            if (review is null) return NotFound();

            if (request.ReviewContent is not null) review.ReviewContent = request.ReviewContent.Trim();
            if (request.QtdStars.HasValue) review.QtdStars = request.QtdStars.Value;

            await context.SaveChangesAsync();

            return Ok(ReviewResponseDto.FromEntity(review));
        }
    }
}
