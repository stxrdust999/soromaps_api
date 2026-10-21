using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Community;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para consulta de exploradores e perfis públicos da comunidade.
 */
public interface ICommunityService
{
    /**
     * Consulta ranking público de exploradores ordenados por índice de contribuição.
     */
    Task<Result<PagedResponseDto<ExplorerListItemDto>>> ListExplorersAsync(
        PaginationQueryDto pagination,
        string? neighborhood = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /**
     * Consulta perfil público com histórico de avaliações, locais aprovados e conquistas.
     */
    Task<Result<CommunityUserProfileDto>> GetPublicProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
