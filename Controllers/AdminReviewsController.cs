using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Admin;
using soromaps_api.DTOs.Common;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelo painel administrativo de moderação de avaliações e remoção de conteúdo.
 * Acesso exclusivo a administradores do sistema.
 */
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminReviewsController(IAdminReviewService adminReviewService) : ApiControllerBase
{
    /**
     * Lista avaliações com detecção de anomalias (outliers de nota, rajadas de avaliações) e filtros avançados.
     */
    [HttpGet("admin/reviews")]
    [EndpointName("adminListReviews")]
    [EndpointSummary("Lista avaliações com filtros avançados para moderação administrativa")]
    [ProducesResponseType(typeof(PagedResponseDto<AdminReviewItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponseDto<AdminReviewItemDto>>> ListReviews(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] AdminReviewQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await adminReviewService.ListReviewsAsync(pagination, query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Remove administrativamente uma avaliação ou comentário, preservando trilha de auditoria e snapshot do texto.
     */
    [HttpPost("admin/removals")]
    [EndpointName("adminCreateRemoval")]
    [EndpointSummary("Aplica remoção administrativa em uma avaliação ou comentário")]
    [ProducesResponseType(typeof(ContentRemovalResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentRemovalResponseDto>> CreateRemoval(
        [FromBody] ContentRemovalRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await adminReviewService.RemoveContentAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /**
     * Consulta o histórico paginado de conteúdos removidos administrativamente.
     */
    [HttpGet("admin/removals")]
    [EndpointName("adminListRemovals")]
    [EndpointSummary("Lista histórico de remoções de conteúdo")]
    [ProducesResponseType(typeof(PagedResponseDto<ContentRemovalResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponseDto<ContentRemovalResponseDto>>> ListRemovals(
        [FromQuery] PaginationQueryDto pagination,
        CancellationToken cancellationToken)
    {
        var result = await adminReviewService.ListRemovalsHistoryAsync(pagination, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
