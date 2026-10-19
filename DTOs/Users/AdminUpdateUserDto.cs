using System.ComponentModel.DataAnnotations;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Users;

public sealed record AdminUpdateUserDto(
    [MaxLength(100, ErrorMessage = "user.name_too_long")]
    string? Name,

    [EmailAddress(ErrorMessage = "user.invalid_email")]
    [MaxLength(255, ErrorMessage = "user.email_too_long")]
    string? Email,

    [MinLength(8, ErrorMessage = "user.password_too_short")]
    [MaxLength(72, ErrorMessage = "user.password_too_long")]
    string? Password,

    UserRole? Role,

    [MaxLength(500, ErrorMessage = "user.avatar_url_too_long")]
    string? AvatarUrl,

    [MaxLength(280, ErrorMessage = "user.biography_too_long")]
    string? Biography,

    [MaxLength(60, ErrorMessage = "user.neighborhood_too_long")]
    string? Neighborhood
);
