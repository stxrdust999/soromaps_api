namespace soromaps_api.DTOs.Categories;

public sealed record CategoryResponseDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
