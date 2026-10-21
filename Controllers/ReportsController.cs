using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reports;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelo recebimento de denúncias e fila de triagem da moderação.
 */
[ApiController]
[Route("api")]
public class ReportsController(IReportService reportService) : ApiControllerBase
{
    /**
     * Registra uma denúncia contra um local, avaliação ou comentário.
     */
    [HttpPost("reports")]
    [Authorize]
    [EndpointName("createReport")]
    [EndpointSummary("Registra uma denúncia contra um local, avaliação ou comentário")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportDto request,
        CancellationToken cancellationToken)
    {
        var result = await reportService.CreateReportAsync(request, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }

    /**
     * Lista denúncias abertas agrupadas por conteúdo alvo para triagem administrativa.
     */
    [HttpGet("admin/reports")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("listAdminGroupedReports")]
    [EndpointSummary("Lista denúncias agrupadas por conteúdo alvo ordenadas por denunciantes distintos")]
    [ProducesResponseType(typeof(PagedResponseDto<AdminGroupedReportItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponseDto<AdminGroupedReportItemDto>>> ListAdminReports(
        [FromQuery] PaginationQueryDto pagination,
        CancellationToken cancellationToken)
    {
        var result = await reportService.ListAdminGroupedReportsAsync(pagination, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }

    /**
     * Descarta denúncias abertas de um alvo mantendo o conteúdo ativo na plataforma.
     */
    [HttpPost("admin/reports/{targetType}/{targetId:guid}/dismiss")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [EndpointName("dismissAdminReports")]
    [EndpointSummary("Descarta as denúncias abertas de um conteúdo mantendo-o publicado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dismiss(
        string targetType,
        Guid targetId,
        [FromBody] DismissReportsDto request,
        CancellationToken cancellationToken)
    {
        var result = await reportService.DismissReportsAsync(targetType, targetId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result.Error!);
    }
}
