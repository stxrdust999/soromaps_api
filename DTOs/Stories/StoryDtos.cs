using System.ComponentModel.DataAnnotations;
using soromaps_api.DTOs.Categories;
using soromaps_api.DTOs.Common;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Stories;

public sealed record StoryPlaceInputDto(
    Guid PlaceId,
    short SortOrder,
    [Required, MaxLength(120)] string AngleTitle,
    [Required] string BodyText,
    [MaxLength(280)] string? LocalTip
);

public sealed record StoryPlaceItemDto(
    Guid PlaceId,
    string PlaceName,
    string Neighborhood,
    CategorySummaryDto Category,
    string? CoverPhotoUrl,
    short SortOrder,
    string AngleTitle,
    string BodyText,
    string? LocalTip
);

public sealed record CreateStoryDto(
    [Required, MaxLength(120)] string Slug,
    [Required, MaxLength(140)] string Title,
    [Required, MaxLength(280)] string Subtitle,
    [MaxLength(60)] string? Neighborhood,
    [Required] string IntroText,
    string? ClosingText,
    StoryOrigin Origin = StoryOrigin.Editorial,
    IReadOnlyList<StoryPlaceInputDto>? Places = null
);

public sealed record UpdateStoryDto(
    [MaxLength(120)] string? Slug = null,
    [MaxLength(140)] string? Title = null,
    [MaxLength(280)] string? Subtitle = null,
    [MaxLength(60)] string? Neighborhood = null,
    string? IntroText = null,
    string? ClosingText = null,
    IReadOnlyList<StoryPlaceInputDto>? Places = null
);

public sealed record StorySummaryDto(
    Guid Id,
    string Slug,
    string Title,
    string Subtitle,
    string? Neighborhood,
    StoryOrigin Origin,
    StoryStatus Status,
    AuthorSummaryDto Author,
    int PlacesCount,
    string? CoverPhotoUrl,
    DateTime? PublishedAt,
    DateTime CreatedAt
);

public sealed record StoryDetailDto(
    Guid Id,
    string Slug,
    string Title,
    string Subtitle,
    string? Neighborhood,
    string IntroText,
    string? ClosingText,
    StoryOrigin Origin,
    StoryStatus Status,
    AuthorSummaryDto Author,
    IReadOnlyList<StoryPlaceItemDto> Places,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
