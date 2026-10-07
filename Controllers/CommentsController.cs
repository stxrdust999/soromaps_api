using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs;
using soromaps_api.Models;

namespace soromaps_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController(AppDbContext context) : ControllerBase
    {
        [HttpGet("review/{reviewId:guid}")]
        [EndpointSummary("Lista os comentários de uma avaliação específica")]
        [ProducesResponseType(typeof(IEnumerable<CommentResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetByReviewId(Guid reviewId)
        {
            var comments = await context.Comments
                .AsNoTracking()
                .Where(c => c.ReviewId == reviewId)
                .Select(c => CommentResponseDto.FromEntity(c))
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost]
        [EndpointSummary("Adiciona um comentário a uma avaliação")]
        [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<CommentResponseDto>> Create(CreateCommentDto request)
        {
            // O UserId virá do Token JWT no futuro
            var currentUserId = Guid.Empty;

            var comment = new Comment
            {
                ReviewId = request.ReviewId,
                UserId = currentUserId,
                CommentContent = request.CommentContent.Trim()
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, CommentResponseDto.FromEntity(comment));
        }

        [HttpDelete("{id:guid}")]
        [EndpointSummary("Deleta um comentário")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await context.Comments.FindAsync(id);
            if (comment is null) return NotFound();

            context.Comments.Remove(comment);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
