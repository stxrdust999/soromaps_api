using System.Linq.Expressions;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Comments;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

public static class CommentMappings
{
    public static readonly Expression<Func<Comment, CommentResponseDto>> ToResponseProjection = comment =>
        new CommentResponseDto(
            comment.Id,
            comment.ReviewId,
            comment.ReplyToId,
            comment.User == null ? null : new AuthorSummaryDto(comment.User.Id, comment.User.Name, comment.User.AvatarUrl),
            comment.Content,
            comment.CreatedAt,
            comment.UpdatedAt
        );

    public static CommentResponseDto ToResponse(this Comment comment) =>
        new(
            comment.Id,
            comment.ReviewId,
            comment.ReplyToId,
            comment.User == null ? null : new AuthorSummaryDto(comment.User.Id, comment.User.Name, comment.User.AvatarUrl),
            comment.Content,
            comment.CreatedAt,
            comment.UpdatedAt
        );

    public static void ApplyUpdate(this Comment comment, UpdateCommentDto request)
    {
        if (request.Content is not null)
            comment.Content = request.Content.Trim();
    }
}
