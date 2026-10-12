using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Users;

public sealed record RegisterUserDto(
    [Required(ErrorMessage = "user.name_required")]
    [MaxLength(100, ErrorMessage = "user.name_too_long")]
    string Name,

    [Required(ErrorMessage = "user.email_required")]
    [EmailAddress(ErrorMessage = "user.invalid_email")]
    [MaxLength(255, ErrorMessage = "user.email_too_long")]
    string Email,

    [Required(ErrorMessage = "user.password_required")]
    [MinLength(8, ErrorMessage = "user.password_too_short")]
    [MaxLength(72, ErrorMessage = "user.password_too_long")]
    string Password
);
