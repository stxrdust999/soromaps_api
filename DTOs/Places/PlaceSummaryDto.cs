using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Tags;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Places;

public sealed record PlaceSummaryDto(
    Guid Id,
    Guid CategoryId,
    CategorySummaryDto Category,
    string Name,
    double Latitude,
    double Longitude,
    string Neighborhood,
    string About,
    bool HasWifi,
    bool IsPetFriendly,
    double RatingAverage,
    int ReviewsCount,
    string? CoverPhotoUrl,
    IReadOnlyList<TagResponseDto> Tags,
    PlaceStatus Status,
    DateTime CreatedAt
);
