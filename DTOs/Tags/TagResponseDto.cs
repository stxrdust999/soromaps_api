using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Tags;

public sealed record TagResponseDto(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAt
);

public sealed record CreateTagDto(
    [Required(ErrorMessage = "tag.name_required")]
    [MaxLength(50, ErrorMessage = "tag.name_too_long")]
    string Name,

    [MaxLength(60, ErrorMessage = "tag.slug_too_long")]
    string? Slug = null
);
