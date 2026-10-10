using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using soromaps_api.Data;
using soromaps_api.Models;
using static soromaps_api.DTOs.CategoryDto;

namespace soromaps_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController (AppDbContext context) : ControllerBase
    {
        [HttpGet]
        [EndpointSummary("Lista todas as categorias")]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
        {
            var categories = await context.Categorys
                .AsNoTracking()
                .OrderBy(c => c.CategoryDesc)
                .Select(c => CategoryResponseDto.FromEntity(c))
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id:guid}")]
        [EndpointSummary("Busca uma categoria pela ID")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryResponseDto>> GetById(Guid id)
        {
            var category = await context.Categorys
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category is null)
                return NotFound();

            return Ok(CategoryResponseDto.FromEntity(category));
        }

        [HttpPost]
        [EndpointSummary("Cadastra uma nova categoria")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<CategoryResponseDto>> Create(CreateCategoryDto request)
        {
            var category = new Category
            {
                CategoryDesc = request.CategoryDesc.Trim()
            };

            context.Categorys.Add(category);
            await context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.CategoryId },
                CategoryResponseDto.FromEntity(category)
            );
        }
    }
}
