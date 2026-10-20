using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Feed;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para agregação do feed dinâmico baseado em locais e silenciamentos.
 */
public interface IFeedService
{
    /**
     * Monta o feed do usuário considerando eventos recentes e excluindo alvos silenciados.
     */
    Task<Result<PagedResponseDto<FeedItemDto>>> GetFeedAsync(FeedQueryDto query, CancellationToken cancellationToken = default);

    /**
     * Lista as preferências ativas de silenciamento do usuário autenticado.
     */
    Task<Result<IReadOnlyList<FeedMuteResponseDto>>> ListMutesAsync(CancellationToken cancellationToken = default);

    /**
     * Adiciona uma regra para ver menos itens de determinado bairro, categoria ou tipo.
     */
    Task<Result<FeedMuteResponseDto>> AddMuteAsync(CreateFeedMuteDto request, CancellationToken cancellationToken = default);

    /**
     * Remove uma regra de silenciamento anteriormente configurada.
     */
    Task<Result> RemoveMuteAsync(Guid id, CancellationToken cancellationToken = default);
}
