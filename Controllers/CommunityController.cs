using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Community;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelo ranking público e perfis da comunidade de exploradores de Sorocaba.
 */
[ApiController]
[Route("api")]
public class CommunityController(ICommunityService communityService) : ApiControllerBase
{
    /**
     * Consulta ranking público de exploradores por índice de contribuição.
     */
    [HttpGet("community/explorers")]
    [AllowAnonymous]
    [EndpointName("listExplorers")]
    [EndpointSummary("Lista ranking público de exploradores")]
    [ProducesResponseType(typeof(PagedResponseDto<ExplorerListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<ExplorerListItemDto>>> ListExplorers(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] string? neighborhood,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await communityService.ListExplorersAsync(pagination, neighborhood, search, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Retorna o perfil público com histórico de contribuições, avaliações, locais e conquistas.
     */
    [HttpGet("community/users/{id:guid}")]
    [AllowAnonymous]
    [EndpointName("getCommunityUserProfile")]
    [EndpointSummary("Retorna o perfil público com histórico de contribuições")]
    [ProducesResponseType(typeof(CommunityUserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommunityUserProfileDto>> GetPublicProfile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await communityService.GetPublicProfileAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
