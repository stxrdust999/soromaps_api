using System.Linq.Expressions;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Reviews;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

/**
 * Utilitários de transformação entre a entidade Review e seus DTOs.
 * Fornece expressões de projeção para o EF Core traduzir em SQL e métodos de extensão para objetos em memória.
 */
public static class ReviewMappings
{
    /**
     * Expressão de projeção otimizada para listagem de avaliações de um local.
     * Gera uma query SQL trazendo os dados da avaliação junto com o resumo do autor via subprojeção,
     * garantindo que dados confidenciais do usuário (e-mail, senha) nunca trafeguem do PostgreSQL para a API.
     */
    public static readonly Expression<Func<Review, ReviewResponseDto>> ToResponseProjection = review =>
        new ReviewResponseDto(
            review.Id,
            review.PlaceId,
            review.User == null ? null : new AuthorSummaryDto(review.User.Id, review.User.Name, review.User.AvatarUrl),
            review.Content,
            review.Rating,
            review.CreatedAt,
            review.UpdatedAt
        );

    /**
     * Converte uma entidade Review carregada na memória para o DTO de resposta detalhado.
     * Trata graciosamente cenários onde o autor foi anonimizado ou excluído (author == null).
     */
    public static ReviewResponseDto ToResponse(this Review review) =>
        new(
            review.Id,
            review.PlaceId,
            review.User == null ? null : new AuthorSummaryDto(review.User.Id, review.User.Name, review.User.AvatarUrl),
            review.Content,
            review.Rating,
            review.CreatedAt,
            review.UpdatedAt
        );

    /**
     * Aplica alterações parciais vindas de UpdateReviewDto sobre a entidade rastreada,
     * atualizando apenas os campos preenchidos e sanitizando o texto da avaliação.
     */
    public static void ApplyUpdate(this Review review, UpdateReviewDto request)
    {
        if (request.Content is not null)
            review.Content = request.Content.Trim();

        if (request.Rating.HasValue)
            review.Rating = request.Rating.Value;
    }
}
