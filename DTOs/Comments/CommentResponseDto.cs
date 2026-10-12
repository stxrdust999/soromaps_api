using soromaps_api.DTOs.Common;

namespace soromaps_api.DTOs.Comments;

public sealed record CommentResponseDto(
    Guid Id,
    Guid ReviewId,
    AuthorSummaryDto? Author,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
