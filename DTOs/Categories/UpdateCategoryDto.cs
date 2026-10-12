using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Categories;

public sealed record UpdateCategoryDto(
    [MaxLength(60, ErrorMessage = "category.name_too_long")]
    string? Name
);
