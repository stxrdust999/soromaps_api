using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para gestão de avaliações de locais e votos de utilidade da comunidade.
 */
public interface IReviewService
{
    /**
     * Lista avaliações ativas de um local ordenadas por relevância, data ou nota.
     */
    Task<Result<PagedResponseDto<ReviewResponseDto>>> ListByPlaceIdAsync(
        Guid placeId,
        PaginationQueryDto pagination,
        string? sort = "useful",
        CancellationToken cancellationToken = default);

    /**
     * Publica uma nova avaliação garantindo unicidade por usuário e local.
     */
    Task<Result<ReviewResponseDto>> CreateAsync(CreateReviewDto request, CancellationToken cancellationToken = default);

    /**
     * Atualiza o texto e a nota da avaliação existente de autoria do usuário.
     */
    Task<Result<ReviewResponseDto>> UpdateAsync(Guid id, UpdateReviewDto request, CancellationToken cancellationToken = default);

    /**
     * Exclui a avaliação e os votos de utilidade associados.
     */
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /**
     * Registra o voto indicando que a avaliação foi útil para outro explorador.
     */
    Task<Result> VoteUsefulAsync(Guid id, CancellationToken cancellationToken = default);

    /**
     * Remove o voto de utilidade anteriormente atribuído à avaliação.
     */
    Task<Result> RemoveUsefulVoteAsync(Guid id, CancellationToken cancellationToken = default);
}
