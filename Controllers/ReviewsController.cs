using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelas avaliações e notas dos locais cadastrados.
 * Permite leitura aberta e restringe a criação de avaliações a usuários autenticados.
 */
public class ReviewsController(IReviewService reviewService) : ApiControllerBase
{
    /**
     * Lista avaliações de um local específico de forma paginada.
     */
    [HttpGet("place/{placeId:guid}")]
    [AllowAnonymous]
    [EndpointName("listReviewsByPlace")]
    [EndpointSummary("Lista as avaliações de um lugar de forma paginada")]
    [ProducesResponseType(typeof(PagedResponseDto<ReviewResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<ReviewResponseDto>>> ListByPlaceId(
        Guid placeId,
        [FromQuery] PaginationQueryDto pagination,
        CancellationToken cancellationToken)
    {
        var result = await reviewService.ListByPlaceIdAsync(placeId, pagination, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Cria uma nova avaliação para um lugar aprovado.
     * Retorna conflito 409 caso o usuário já tenha avaliado o local anteriormente.
     */
    [HttpPost]
    [Authorize]
    [EndpointName("createReview")]
    [EndpointSummary("Cadastra uma nova avaliação para um lugar")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponseDto>> Create([FromBody] CreateReviewDto request, CancellationToken cancellationToken)
    {
        var result = await reviewService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /**
     * Atualiza o conteúdo ou nota de uma avaliação (restrito ao autor ou administrador).
     */
    [HttpPatch("{id:guid}")]
    [Authorize]
    [EndpointName("updateReview")]
    [EndpointSummary("Atualiza uma avaliação existente")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> Update(Guid id, [FromBody] UpdateReviewDto request, CancellationToken cancellationToken)
    {
        var result = await reviewService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Remove uma avaliação (restrito ao autor ou administrador).
     */
    [HttpDelete("{id:guid}")]
    [Authorize]
    [EndpointName("deleteReview")]
    [EndpointSummary("Remove uma avaliação")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await reviewService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
