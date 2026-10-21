using soromaps_api.DTOs.Achievements;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Places;
using soromaps_api.DTOs.Reviews;

namespace soromaps_api.DTOs.Community;

public sealed record ExplorerListItemDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    string? Neighborhood,
    int ContributionScore,
    int ReviewsCount,
    int ApprovedPlacesCount,
    int VisitsCount,
    int AchievementsCount,
    string TitleBadge,
    bool IsVerifiedExplorer,
    DateTime JoinedAt
);

public sealed record CommunityUserProfileDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    string? Biography,
    string? Neighborhood,
    int ContributionScore,
    int VisitsCount,
    int ReviewsCount,
    int ApprovedPlacesCount,
    int AchievementsCount,
    string TitleBadge,
    bool IsVerifiedExplorer,
    DateTime JoinedAt,
    IReadOnlyList<ReviewResponseDto> RecentReviews,
    IReadOnlyList<PlaceSummaryDto> ApprovedPlaces,
    IReadOnlyList<UserAchievementProgressDto> UnlockedAchievements
);
