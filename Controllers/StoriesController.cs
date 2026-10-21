using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Stories;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelas pautas editoriais, roteiros temáticos e curadorias de locais.
 */
[ApiController]
[Route("api")]
public class StoriesController(IStoryService storyService) : ApiControllerBase
{
    /**
     * Lista pautas editoriais de forma paginada.
     */
    [HttpGet("stories")]
    [AllowAnonymous]
    [EndpointName("listStories")]
    [EndpointSummary("Lista pautas editoriais publicadas de forma paginada")]
    [ProducesResponseType(typeof(PagedResponseDto<StorySummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<StorySummaryDto>>> List(
        [FromQuery] PaginationQueryDto pagination,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await storyService.ListAsync(pagination, status, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Obtém os detalhes completos de uma pauta pelo seu slug amigável.
     */
    [HttpGet("stories/{slug}")]
    [AllowAnonymous]
    [EndpointName("getStoryBySlug")]
    [EndpointSummary("Busca detalhes de uma pauta através de seu slug")]
    [ProducesResponseType(typeof(StoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoryDetailDto>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await storyService.GetBySlugAsync(slug, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Cria um novo rascunho de pauta editorial.
     */
    [HttpPost("stories")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("createStory")]
    [EndpointSummary("Cria um rascunho de pauta editorial")]
    [ProducesResponseType(typeof(StoryDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StoryDetailDto>> Create(
        [FromBody] CreateStoryDto request,
        CancellationToken cancellationToken)
    {
        var result = await storyService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /**
     * Atualiza dados de conteúdo e locais vinculados a uma pauta.
     */
    [HttpPatch("stories/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("updateStory")]
    [EndpointSummary("Atualiza uma pauta editorial existente")]
    [ProducesResponseType(typeof(StoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoryDetailDto>> Update(
        Guid id,
        [FromBody] UpdateStoryDto request,
        CancellationToken cancellationToken)
    {
        var result = await storyService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Publica a pauta após verificar que todos os locais associados estão aprovados.
     */
    [HttpPost("stories/{id:guid}/publish")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("publishStory")]
    [EndpointSummary("Publica uma pauta editorial após validar locais aprovados")]
    [ProducesResponseType(typeof(StoryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoryDetailDto>> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await storyService.PublishAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
