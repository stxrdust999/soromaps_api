using System.ComponentModel.DataAnnotations;
using soromaps_api.DTOs.Categories;

namespace soromaps_api.DTOs.Activity;

public sealed record CheckInVisitDto(
    [Range(-90.0, 90.0, ErrorMessage = "visit.invalid_latitude")]
    double Latitude,

    [Range(-180.0, 180.0, ErrorMessage = "visit.invalid_longitude")]
    double Longitude
);

public sealed record VisitPlaceSummaryDto(
    Guid Id,
    string Name,
    string Neighborhood,
    CategorySummaryDto Category
);

public sealed record VisitResponseDto(
    Guid Id,
    VisitPlaceSummaryDto Place,
    DateOnly VisitedOn,
    bool HasReviewed,
    DateTime CreatedAt
);

public sealed record FavoriteResponseDto(
    Guid PlaceId,
    string PlaceName,
    string Neighborhood,
    CategorySummaryDto Category,
    double RatingAverage,
    int ReviewsCount,
    string? CoverPhotoUrl,
    DateTime FavoritedAt
);

public sealed record MonthlyVisitsCountDto(
    string Month, // "yyyy-MM"
    int Count
);

public sealed record CategoryVisitsDistributionDto(
    string CategoryName,
    string Color,
    int Count
);

public sealed record NeighborhoodCoverageDto(
    int VisitedNeighborhoodsCount,
    int TotalActiveNeighborhoodsCount,
    double CoveragePercentage
);

public sealed record RatingDistributionDto(
    int Rating,
    int Count
);

public sealed record UserStatsDto(
    int TotalVisits,
    int TotalFavorites,
    int TotalReviews,
    IReadOnlyList<MonthlyVisitsCountDto> VisitsByMonth,
    IReadOnlyList<CategoryVisitsDistributionDto> VisitsByCategory,
    NeighborhoodCoverageDto NeighborhoodCoverage,
    IReadOnlyList<RatingDistributionDto> RatingsGiven
);

public sealed record UserVerificationStatusDto(
    int VisitsCount,
    int RequiredVisits,
    int PublishedReviewsCount,
    int RequiredReviews,
    int RemovedReviewsCount,
    int MaxAllowedRemovedReviews,
    int AccountAgeDays,
    int RequiredAccountAgeDays,
    bool IsVerified
);
