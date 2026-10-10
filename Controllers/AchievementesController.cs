using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.DTOs;
using soromaps_api.Models;

namespace soromaps_api.Controllers
{
    public class AchievementesController
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AchievementsController(AppDbContext context) : ControllerBase
        {
            [HttpGet]
            [EndpointSummary("Lista todas as conquistas disponíveis")]
            [ProducesResponseType(typeof(IEnumerable<AchievementResponseDto>), StatusCodes.Status200OK)]
            public async Task<ActionResult<IEnumerable<AchievementResponseDto>>> GetAll()
            {
                var achievements = await context.Achievements
                    .AsNoTracking()
                    .OrderBy(a => a.AchievementName)
                    .Select(a => AchievementResponseDto.FromEntity(a))
                    .ToListAsync();

                return Ok(achievements);
            }

            [HttpPost]
            [EndpointSummary("Cria uma nova conquista")]
            [ProducesResponseType(typeof(AchievementResponseDto), StatusCodes.Status201Created)]
            public async Task<ActionResult<AchievementResponseDto>> Create(CreateAchievementDto request)
            {
                var achievement = new Achievement
                {
                    AchievementName = request.AchievementName.Trim(),
                    AchievementDesc = request.AchievementDesc.Trim()
                };

                context.Achievements.Add(achievement);
                await context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status201Created, AchievementResponseDto.FromEntity(achievement));
            }
        }
    }
}
