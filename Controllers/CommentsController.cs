using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.DTOs.Comments;
using soromaps_api.DTOs.Common;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelos comentários deixados em avaliações de locais.
 * Permite leitura aberta e restringe criação e remoção a usuários autorizados.
 */
public class CommentsController(ICommentService commentService) : ApiControllerBase
{
    /**
     * Lista comentários de uma avaliação de forma paginada.
     */
    [HttpGet("review/{reviewId:guid}")]
    [AllowAnonymous]
    [EndpointSummary("Lista os comentários de uma avaliação de forma paginada")]
    [ProducesResponseType(typeof(PagedResponseDto<CommentResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<CommentResponseDto>>> ListByReviewId(
        Guid reviewId,
        [FromQuery] PaginationQueryDto pagination,
        CancellationToken cancellationToken)
    {
        var result = await commentService.ListByReviewIdAsync(reviewId, pagination, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Adiciona um comentário em uma avaliação existente.
     */
    [HttpPost]
    [Authorize]
    [EndpointSummary("Adiciona um comentário a uma avaliação")]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommentResponseDto>> Create([FromBody] CreateCommentDto request, CancellationToken cancellationToken)
    {
        var result = await commentService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /**
     * Atualiza o texto de um comentário (restrito ao autor ou administrador).
     */
    [HttpPatch("{id:guid}")]
    [Authorize]
    [EndpointSummary("Atualiza um comentário existente")]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentResponseDto>> Update(Guid id, [FromBody] UpdateCommentDto request, CancellationToken cancellationToken)
    {
        var result = await commentService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Exclui um comentário do sistema (restrito ao autor ou administrador).
     */
    [HttpDelete("{id:guid}")]
    [Authorize]
    [EndpointSummary("Remove um comentário")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await commentService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
