using System.ComponentModel.DataAnnotations;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.DTOs.Tags;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Places;

public sealed record PlacePhotoResponseDto(
    Guid Id,
    string Url,
    string? StoragePath,
    bool IsCover,
    DateTime CreatedAt
);

public sealed record ConfirmPlacePhotoDto(
    [Required(ErrorMessage = "place.photo_url_required")]
    [MaxLength(600, ErrorMessage = "place.photo_url_too_long")]
    string Url,

    [MaxLength(300, ErrorMessage = "place.storage_path_too_long")]
    string? StoragePath = null,

    bool IsCover = false
);

public sealed record PlaceListQueryDto(
    Guid? CategoryId = null,
    string? CategorySlug = null,
    string? TagSlug = null,
    string? Neighborhood = null,
    string? Search = null,
    string? Bbox = null,
    string? Sort = "recent"
);

public sealed record PlaceResponseDto(
    Guid Id,
    Guid CategoryId,
    CategorySummaryDto Category,
    AuthorSummaryDto? Author,
    string Name,
    double Latitude,
    double Longitude,
    string Neighborhood,
    string About,
    string Description,
    bool HasWifi,
    bool IsPetFriendly,
    string? BestTime,
    string? LocalSecret,
    double RatingAverage,
    int ReviewsCount,
    string? CoverPhotoUrl,
    IReadOnlyList<PlacePhotoResponseDto> Photos,
    IReadOnlyList<TagResponseDto> Tags,
    PlaceStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
