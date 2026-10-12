using soromaps_api.DTOs.Common;

namespace soromaps_api.DTOs.Reviews;

public sealed record ReviewResponseDto(
    Guid Id,
    Guid PlaceId,
    AuthorSummaryDto? Author,
    string Content,
    short Rating,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
