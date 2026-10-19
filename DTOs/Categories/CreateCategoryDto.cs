using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Categories;

public sealed record CreateCategoryDto(
    [Required(ErrorMessage = "category.name_required")]
    [MaxLength(60, ErrorMessage = "category.name_too_long")]
    string Name,

    [MaxLength(60, ErrorMessage = "category.slug_too_long")]
    string? Slug = null,

    [MaxLength(40, ErrorMessage = "category.icon_too_long")]
    string? Icon = "map-pin",

    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "category.invalid_color")]
    string? Color = "#2563EB",

    short? SortOrder = null,

    bool IsActive = true
);
