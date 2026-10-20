using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reports;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para triagem e moderação de denúncias da comunidade.
 */
public interface IReportService
{
    /**
     * Registra uma denúncia de usuário contra um local, avaliação ou comentário.
     */
    Task<Result> CreateReportAsync(CreateReportDto request, CancellationToken cancellationToken = default);

    /**
     * Lista denúncias abertas agrupadas por conteúdo alvo para ação na fila administrativa.
     */
    Task<Result<PagedResponseDto<AdminGroupedReportItemDto>>> ListAdminGroupedReportsAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default);

    /**
     * Descarta denúncias abertas de um alvo, mantendo o conteúdo no ar.
     */
    Task<Result> DismissReportsAsync(
        string targetType,
        Guid targetId,
        DismissReportsDto request,
        CancellationToken cancellationToken = default);
}
