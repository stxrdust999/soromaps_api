using soromaps_api.Models;

namespace soromaps_api.DTOs
{
    public record CommentResponseDto(
         Guid CommentId,
         Guid UserId,
         Guid ReviewId,
         string CommentContent
     )
    {
        public static CommentResponseDto FromEntity(Comment comment) =>
            new(
                comment.CommentId,
                comment.UserId,
                comment.ReviewId,
                comment.CommentContent
            );
    }

    public record CreateCommentDto(
        Guid ReviewId,
        string CommentContent
    );

    public record UpdateCommentDto(
        string? CommentContent
    );
}
