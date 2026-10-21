using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.DTOs.Activity;
using soromaps_api.DTOs.Common;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelo registro e consulta de atividades do explorador.
 * Gerencia check-ins presenciais com validação de geolocalização, favoritos e status de verificação.
 */
[Authorize]
[ApiController]
[Route("api")]
public class ActivityController(IActivityService activityService) : ApiControllerBase
{
    /**
     * Registra check-in em um local após validar proximidade máxima de 200m e unicidade diária.
     */
    [HttpPost("places/{placeId:guid}/visits")]
    [EndpointName("checkInVisit")]
    [EndpointSummary("Registra um check-in/visita em um local")]
    [ProducesResponseType(typeof(VisitResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VisitResponseDto>> CheckIn(
        Guid placeId,
        [FromBody] CheckInVisitDto request,
        CancellationToken cancellationToken)
    {
        var result = await activityService.CheckInAsync(placeId, request, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /**
     * Lista o histórico paginado de visitas e check-ins realizados pelo usuário logado.
     */
    [HttpGet("users/me/visits")]
    [EndpointName("listMyVisits")]
    [EndpointSummary("Lista histórico de check-ins e visitas do usuário autenticado")]
    [ProducesResponseType(typeof(PagedResponseDto<VisitResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponseDto<VisitResponseDto>>> ListMyVisits(
        [FromQuery] PaginationQueryDto pagination,
        CancellationToken cancellationToken)
    {
        var result = await activityService.ListMyVisitsAsync(pagination, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Salva um local aprovado na lista de favoritos do usuário.
     */
    [HttpPost("places/{placeId:guid}/favorite")]
    [EndpointName("addFavoritePlace")]
    [EndpointSummary("Salva um local nos favoritos do usuário")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFavorite(Guid placeId, CancellationToken cancellationToken)
    {
        var result = await activityService.AddFavoriteAsync(placeId, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    /**
     * Remove um local dos favoritos do usuário autenticado.
     */
    [HttpDelete("places/{placeId:guid}/favorite")]
    [EndpointName("removeFavoritePlace")]
    [EndpointSummary("Remove um local dos favoritos do usuário")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFavorite(Guid placeId, CancellationToken cancellationToken)
    {
        var result = await activityService.RemoveFavoriteAsync(placeId, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    /**
     * Lista locais salvos nos favoritos do usuário autenticado, com filtros opcionais de categoria e bairro.
     */
    [HttpGet("users/me/favorites")]
    [EndpointName("listMyFavorites")]
    [EndpointSummary("Lista locais favoritos do usuário autenticado")]
    [ProducesResponseType(typeof(PagedResponseDto<FavoriteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponseDto<FavoriteResponseDto>>> ListMyFavorites(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? neighborhood,
        CancellationToken cancellationToken)
    {
        var result = await activityService.ListMyFavoritesAsync(pagination, categoryId, neighborhood, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Consolida estatísticas de exploração (visitas por mês, por categoria, cobertura de bairros e notas dadas).
     */
    [HttpGet("users/me/stats")]
    [EndpointName("getMyStats")]
    [EndpointSummary("Obtém estatísticas consolidadas e métricas do perfil do usuário autenticado")]
    [ProducesResponseType(typeof(UserStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserStatsDto>> GetMyStats(CancellationToken cancellationToken)
    {
        var result = await activityService.GetMyStatsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Consulta o progresso do usuário em relação aos requisitos para obter o selo de Explorador Verificado.
     */
    [HttpGet("users/me/verification")]
    [EndpointName("getMyVerificationStatus")]
    [EndpointSummary("Consulta o progresso e critérios para o selo de Explorador Verificado")]
    [ProducesResponseType(typeof(UserVerificationStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserVerificationStatusDto>> GetMyVerificationStatus(CancellationToken cancellationToken)
    {
        var result = await activityService.GetMyVerificationStatusAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
