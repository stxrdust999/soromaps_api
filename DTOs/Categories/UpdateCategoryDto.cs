using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Categories;

public sealed record UpdateCategoryDto(
    [MaxLength(60, ErrorMessage = "category.name_too_long")]
    string? Name = null,

    [MaxLength(60, ErrorMessage = "category.slug_too_long")]
    string? Slug = null,

    [MaxLength(40, ErrorMessage = "category.icon_too_long")]
    string? Icon = null,

    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "category.invalid_color")]
    string? Color = null,

    short? SortOrder = null,

    bool? IsActive = null
);

public sealed record ReorderCategoriesDto(
    [Required(ErrorMessage = "category.order_required")]
    [MinLength(1, ErrorMessage = "category.order_empty")]
    IReadOnlyList<Guid> CategoryIds
);
