using soromaps_api.DTOs.Admin;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para consolidação de estatísticas operacionais do dashboard administrativo.
 */
public interface IAdminDashboardService
{
    /**
     * Consolida filas de atenção, métricas de crescimento e séries temporais contínuas para o período.
     */
    Task<Result<AdminDashboardStatsDto>> GetStatsAsync(string period, CancellationToken cancellationToken = default);
}
