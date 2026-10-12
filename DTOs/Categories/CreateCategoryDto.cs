using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Categories;

public sealed record CreateCategoryDto(
    [Required(ErrorMessage = "category.name_required")]
    [MaxLength(60, ErrorMessage = "category.name_too_long")]
    string Name
);
