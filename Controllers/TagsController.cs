using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Tags;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController(ITagService tagService) : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointName("listTags")]
    [EndpointSummary("Lista todas as tags disponíveis no catálogo")]
    [ProducesResponseType(typeof(IReadOnlyList<TagResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagResponseDto>>> ListAll(CancellationToken cancellationToken)
    {
        var result = await tagService.ListAllAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("createTag")]
    [EndpointSummary("Cadastra uma nova tag (Administrador)")]
    [ProducesResponseType(typeof(TagResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagResponseDto>> Create([FromBody] CreateTagDto request, CancellationToken cancellationToken)
    {
        var result = await tagService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("deleteTag")]
    [EndpointSummary("Remove uma tag pelo ID (Administrador)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await tagService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
