using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Users;

public sealed record UpdateUserDto(
    [MaxLength(100, ErrorMessage = "user.name_too_long")]
    string? Name,

    [MaxLength(500, ErrorMessage = "user.avatar_url_too_long")]
    string? AvatarUrl,

    [MaxLength(280, ErrorMessage = "user.biography_too_long")]
    string? Biography,

    [MaxLength(60, ErrorMessage = "user.neighborhood_too_long")]
    string? Neighborhood
);
