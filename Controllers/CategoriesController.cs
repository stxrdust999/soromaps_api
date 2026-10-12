using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelas rotas de categorias de lugares.
 * Permite leitura pública e restringe criação/atualização/remoção a Administradores.
 */
public class CategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    /**
     * Lista todas as categorias com paginação e ordenação alfabética.
     */
    [HttpGet]
    [AllowAnonymous]
    [EndpointName("listCategories")]
    [EndpointSummary("Lista todas as categorias de forma paginada")]
    [ProducesResponseType(typeof(PagedResponseDto<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<CategoryResponseDto>>> List([FromQuery] PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        var result = await categoryService.ListAsync(pagination, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Busca detalhes de uma categoria por ID.
     */
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointName("getCategoryById")]
    [EndpointSummary("Busca uma categoria pelo ID")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await categoryService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Cadastra uma nova categoria no sistema (restrito a Administradores).
     */
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("createCategory")]
    [EndpointSummary("Cadastra uma nova categoria (Administrador)")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponseDto>> Create([FromBody] CreateCategoryDto request, CancellationToken cancellationToken)
    {
        var result = await categoryService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /**
     * Atualiza o nome de uma categoria (restrito a Administradores).
     */
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("updateCategory")]
    [EndpointSummary("Atualiza parcialmente uma categoria (Administrador)")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponseDto>> Update(Guid id, [FromBody] UpdateCategoryDto request, CancellationToken cancellationToken)
    {
        var result = await categoryService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Remove uma categoria se não possuir lugares vinculados (restrito a Administradores).
     */
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("deleteCategory")]
    [EndpointSummary("Remove uma categoria (Administrador)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await categoryService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
