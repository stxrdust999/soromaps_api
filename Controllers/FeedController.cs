using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Feed;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelo feed dinâmico baseado em locais e silenciamento de preferências.
 */
[Authorize]
public class FeedController(IFeedService feedService) : ApiControllerBase
{
    /**
     * Retorna os eventos recentes de locais agregados no feed dinâmico do usuário.
     */
    [HttpGet("feed")]
    [EndpointName("getFeed")]
    [EndpointSummary("Retorna o feed dinâmico baseado em lugares")]
    [ProducesResponseType(typeof(PagedResponseDto<FeedItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponseDto<FeedItemDto>>> GetFeed(
        [FromQuery] FeedQueryDto query,
        CancellationToken cancellationToken)
    {
        var result = await feedService.GetFeedAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Lista todas as regras de silenciamento configuradas pelo usuário logado.
     */
    [HttpGet("feed/mutes")]
    [EndpointName("listFeedMutes")]
    [EndpointSummary("Lista as regras de silenciamento do usuário")]
    [ProducesResponseType(typeof(IReadOnlyList<FeedMuteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FeedMuteResponseDto>>> ListMutes(CancellationToken cancellationToken)
    {
        var result = await feedService.ListMutesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Adiciona um novo silenciamento (ex: silenciar determinado bairro ou categoria).
     */
    [HttpPost("feed/mutes")]
    [EndpointName("addFeedMute")]
    [EndpointSummary("Cria um silenciamento para ver menos de determinado escopo")]
    [ProducesResponseType(typeof(FeedMuteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeedMuteResponseDto>> AddMute(
        [FromBody] CreateFeedMuteDto request,
        CancellationToken cancellationToken)
    {
        var result = await feedService.AddMuteAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /**
     * Remove uma regra de silenciamento restaurando a exibição no feed.
     */
    [HttpDelete("feed/mutes/{id:guid}")]
    [EndpointName("removeFeedMute")]
    [EndpointSummary("Remove um silenciamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMute(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await feedService.RemoveMuteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
