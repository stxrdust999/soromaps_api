using System.Linq.Expressions;
using soromaps_api.DTOs.Comments;
using soromaps_api.DTOs.Common;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

/**
 * Utilitários de transformação entre a entidade Comment e seus DTOs.
 * Fornece expressões de projeção para o EF Core traduzir em SQL e métodos de extensão para objetos em memória.
 */
public static class CommentMappings
{
    /**
     * Expressão de projeção otimizada para listagem de comentários de uma avaliação.
     * Gera uma query SQL trazendo o comentário e a identificação resumida e pública do autor,
     * prevenindo vazamento de informações privadas ou sobrecarga de campos desnecessários.
     */
    public static readonly Expression<Func<Comment, CommentResponseDto>> ToResponseProjection = comment =>
        new CommentResponseDto(
            comment.Id,
            comment.ReviewId,
            comment.User == null ? null : new AuthorSummaryDto(comment.User.Id, comment.User.Name, comment.User.AvatarUrl),
            comment.Content,
            comment.CreatedAt,
            comment.UpdatedAt
        );

    /**
     * Converte uma entidade Comment carregada na memória para CommentResponseDto.
     * Trata o autor nulo caso a conta vinculada tenha sido excluída com anonimização.
     */
    public static CommentResponseDto ToResponse(this Comment comment) =>
        new(
            comment.Id,
            comment.ReviewId,
            comment.User == null ? null : new AuthorSummaryDto(comment.User.Id, comment.User.Name, comment.User.AvatarUrl),
            comment.Content,
            comment.CreatedAt,
            comment.UpdatedAt
        );

    /**
     * Aplica alterações de texto vindas de UpdateCommentDto sobre a entidade rastreada.
     */
    public static void ApplyUpdate(this Comment comment, UpdateCommentDto request)
    {
        comment.Content = request.Content.Trim();
    }
}
