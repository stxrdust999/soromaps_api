using soromaps_api.DTOs.Comments;
using soromaps_api.DTOs.Common;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para comentários e discussões em avaliações de locais.
 */
public interface ICommentService
{
    /**
     * Lista comentários de uma avaliação de forma paginada com suas respectivas respostas de primeiro nível.
     */
    Task<Result<PagedResponseDto<CommentResponseDto>>> ListByReviewIdAsync(Guid reviewId, PaginationQueryDto pagination, CancellationToken cancellationToken = default);

    /**
     * Publica um novo comentário em uma avaliação ou responde a um comentário pai.
     */
    Task<Result<CommentResponseDto>> CreateAsync(CreateCommentDto request, CancellationToken cancellationToken = default);

    /**
     * Altera o conteúdo textual de um comentário de autoria do usuário autenticado.
     */
    Task<Result<CommentResponseDto>> UpdateAsync(Guid id, UpdateCommentDto request, CancellationToken cancellationToken = default);

    /**
     * Exclui o comentário do sistema.
     */
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
