using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using soromaps_api.Constants;
using soromaps_api.DTOs.Admin;
using soromaps_api.Services.Interfaces;

namespace soromaps_api.Controllers;

/**
 * Controller responsável pelos dados consolidados do dashboard administrativo.
 */
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route("api")]
public class AdminDashboardController(IAdminDashboardService adminDashboardService) : ApiControllerBase
{
    /**
     * Retorna indicadores operacionais, filas de atenção e séries temporais do painel administrativo.
     */
    [HttpGet("admin/stats")]
    [EndpointName("getAdminDashboardStats")]
    [EndpointSummary("Retorna os indicadores e séries temporais do dashboard administrativo")]
    [ProducesResponseType(typeof(AdminDashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetStats(
        [FromQuery] string period = "30d",
        CancellationToken cancellationToken = default)
    {
        var result = await adminDashboardService.GetStatsAsync(period, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result.Error!);
    }
}
