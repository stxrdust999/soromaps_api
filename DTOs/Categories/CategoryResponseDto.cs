namespace soromaps_api.DTOs.Categories;

public sealed record CategorySummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Icon,
    string Color
);

public sealed record CategoryResponseDto(
    Guid Id,
    string Name,
    string Slug,
    string Icon,
    string Color,
    short SortOrder,
    bool IsActive,
    int PlacesCount,
    int NewPlacesThisWeekCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
