using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Moderation;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

[ApiController]
[Route("api/admin/moderation")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class ModerationController(IModerationService moderationService) : ApiControllerBase
{
    [HttpGet("places")]
    [EndpointName("listModerationPlaces")]
    [EndpointSummary("Lista a fila de moderação de lugares com estatísticas do autor, completude e possíveis duplicatas")]
    [ProducesResponseType(typeof(PagedResponseDto<ModerationPlaceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponseDto<ModerationPlaceItemDto>>> ListQueue(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] ModerationQueueQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await moderationService.ListQueueAsync(pagination, query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPost("places/{placeId:guid}/decisions")]
    [EndpointName("decidePlaceModeration")]
    [EndpointSummary("Registra uma decisão de moderação (aprovar, devolver ou rejeitar) para um lugar")]
    [ProducesResponseType(typeof(ModerationDecisionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ModerationDecisionResponseDto>> Decide(
        Guid placeId,
        [FromBody] ModerationDecisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await moderationService.DecideAsync(placeId, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPost("decisions/batch")]
    [EndpointName("batchDecidePlaceModeration")]
    [EndpointSummary("Registra decisão de moderação em lote para múltiplos lugares")]
    [ProducesResponseType(typeof(IReadOnlyList<ModerationDecisionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ModerationDecisionResponseDto>>> BatchDecide(
        [FromBody] BatchModerationDecisionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await moderationService.BatchDecideAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPost("places/{placeId:guid}/undo")]
    [EndpointName("undoPlaceModerationDecision")]
    [EndpointSummary("Reverte a última decisão de moderação tomada há menos de 24 horas")]
    [ProducesResponseType(typeof(ModerationDecisionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ModerationDecisionResponseDto>> Undo(
        Guid placeId,
        CancellationToken cancellationToken)
    {
        var result = await moderationService.UndoAsync(placeId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpGet("history")]
    [EndpointName("listModerationHistory")]
    [EndpointSummary("Lista o histórico paginado de decisões de moderação")]
    [ProducesResponseType(typeof(PagedResponseDto<ModerationDecisionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponseDto<ModerationDecisionResponseDto>>> ListHistory(
        [FromQuery] PaginationQueryDto pagination,
        CancellationToken cancellationToken)
    {
        var result = await moderationService.ListHistoryAsync(pagination, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
