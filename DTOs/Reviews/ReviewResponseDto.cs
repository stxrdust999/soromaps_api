using soromaps_api.DTOs.Common;

namespace soromaps_api.DTOs.Reviews;

public sealed record ReviewAuthorDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    int AchievementsCount,
    bool IsVerifiedExplorer,
    bool VisitedPlace
);

public sealed record ReviewResponseDto(
    Guid Id,
    Guid PlaceId,
    ReviewAuthorDto? Author,
    string Content,
    int Rating,
    int UsefulVotesCount,
    bool MarkedUsefulByMe,
    int CommentsCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? PlaceName = null,
    string? PlaceCategory = null
);
