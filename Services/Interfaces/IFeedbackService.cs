using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Feedback;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para recepção e consulta de feedbacks da plataforma.
 */
public interface IFeedbackService
{
    /**
     * Envia um feedback do usuário (bug, sugestão, elogio, conteúdo).
     */
    Task<Result<FeedbackResponseDto>> CreateAsync(CreateFeedbackDto request, CancellationToken cancellationToken = default);

    /**
     * Lista feedbacks recebidos de forma paginada para administradores.
     */
    Task<Result<PagedResponseDto<FeedbackResponseDto>>> ListAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default);
}
