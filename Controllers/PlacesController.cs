using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelos pontos de interesse e locais no mapa.
 * Expõe busca pública de pontos aprovados, submissão de locais e moderação exclusiva para administradores.
 */
public class PlacesController(IPlaceService placeService) : ApiControllerBase
{
    /**
     * Lista lugares aprovados de forma paginada para o mapa.
     * Suporta filtro opcional por categoria.
     */
    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Lista todos os lugares aprovados de forma paginada")]
    [ProducesResponseType(typeof(PagedResponseDto<PlaceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<PlaceSummaryDto>>> List(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] Guid? categoryId,
        CancellationToken cancellationToken)
    {
        var result = await placeService.ListApprovedAsync(pagination, categoryId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Obtém os detalhes completos de um lugar pelo seu ID.
     */
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Busca um lugar pelo ID")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await placeService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Cadastra uma nova sugestão de lugar no mapa (inicia como 'Pending').
     */
    [HttpPost]
    [Authorize]
    [EndpointSummary("Cadastra um novo lugar")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlaceResponseDto>> Create([FromBody] CreatePlaceDto request, CancellationToken cancellationToken)
    {
        var result = await placeService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /**
     * Atualiza dados de um lugar existente (restrito ao autor ou administrador).
     */
    [HttpPatch("{id:guid}")]
    [Authorize]
    [EndpointSummary("Atualiza parcialmente os dados de um lugar")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceResponseDto>> Update(Guid id, [FromBody] UpdatePlaceDto request, CancellationToken cancellationToken)
    {
        var result = await placeService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Altera o status de moderação do local (exclusivo para Administradores).
     */
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointSummary("Atualiza o status de aprovação de um lugar (Administrador)")]
    [ProducesResponseType(typeof(PlaceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceResponseDto>> UpdateStatus(Guid id, [FromBody] UpdatePlaceStatusDto request, CancellationToken cancellationToken)
    {
        var result = await placeService.UpdateStatusAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Exclui um lugar do sistema (restrito ao autor ou administrador).
     */
    [HttpDelete("{id:guid}")]
    [Authorize]
    [EndpointSummary("Remove um lugar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await placeService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
