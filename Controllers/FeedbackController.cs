using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Feedback;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelo recebimento e consulta de feedbacks da plataforma.
 */
[ApiController]
[Route("api")]
public class FeedbackController(IFeedbackService feedbackService) : ApiControllerBase
{
    /**
     * Envia um feedback (sugestão, bug, elogio ou conteúdo) sobre a plataforma.
     */
    [HttpPost("feedback")]
    [Authorize]
    [EndpointName("createFeedback")]
    [EndpointSummary("Registra um feedback do usuário")]
    [ProducesResponseType(typeof(FeedbackResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FeedbackResponseDto>> Create(
        [FromBody] CreateFeedbackDto request,
        CancellationToken cancellationToken)
    {
        var result = await feedbackService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result.Error!);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /**
     * Lista feedbacks recebidos de forma paginada para análise da equipe administradora.
     */
    [HttpGet("admin/feedback")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("listAdminFeedback")]
    [EndpointSummary("Lista feedbacks de usuários de forma paginada")]
    [ProducesResponseType(typeof(PagedResponseDto<FeedbackResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponseDto<FeedbackResponseDto>>> ListAdmin(
        [FromQuery] PaginationQueryDto pagination,
        CancellationToken cancellationToken)
    {
        var result = await feedbackService.ListAsync(pagination, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
