using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Stories;
using soromaps_api.Results;

namespace soromaps_api.Services.Interfaces;

/**
 * Contrato de serviço para curadoria e publicação de pautas editoriais e roteiros.
 */
public interface IStoryService
{
    /**
     * Lista pautas editoriais publicadas (ou rascunhos para administradores).
     */
    Task<Result<PagedResponseDto<StorySummaryDto>>> ListAsync(
        PaginationQueryDto pagination,
        string? status = null,
        CancellationToken cancellationToken = default);

    /**
     * Obtém os detalhes completos de uma pauta através de seu slug amigável.
     */
    Task<Result<StoryDetailDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /**
     * Cria um novo rascunho de pauta editorial.
     */
    Task<Result<StoryDetailDto>> CreateAsync(CreateStoryDto request, CancellationToken cancellationToken = default);

    /**
     * Atualiza dados e locais vinculados a uma pauta em rascunho ou publicada.
     */
    Task<Result<StoryDetailDto>> UpdateAsync(Guid id, UpdateStoryDto request, CancellationToken cancellationToken = default);

    /**
     * Valida locais vinculados e publica a pauta editorial na plataforma.
     */
    Task<Result<StoryDetailDto>> PublishAsync(Guid id, CancellationToken cancellationToken = default);
}
