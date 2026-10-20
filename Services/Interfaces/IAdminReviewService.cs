using soromaps_api.DTOs.Admin;
using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para auditoria administrativa de avaliações e remoção de conteúdo ilícito.
 */
public interface IAdminReviewService
{
    /**
     * Consulta avaliações no painel de moderação com sinalização heurística de notas discrepantes ou rajadas.
     */
    Task<Result<PagedResponseDto<AdminReviewItemDto>>> ListReviewsAsync(
        PaginationQueryDto pagination,
        AdminReviewQueryDto query,
        CancellationToken cancellationToken = default);

    /**
     * Remove avaliações ou comentários com registro obrigatório de justificativa e nota técnica do moderador.
     */
    Task<Result<ContentRemovalResponseDto>> RemoveContentAsync(
        ContentRemovalRequestDto request,
        CancellationToken cancellationToken = default);

    /**
     * Consulta o histórico paginado de ações moderatórias de remoção de conteúdo.
     */
    Task<Result<PagedResponseDto<ContentRemovalResponseDto>>> ListRemovalsHistoryAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default);
}
