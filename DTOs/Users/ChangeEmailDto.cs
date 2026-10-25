using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Users;

public sealed record ChangeEmailDto(
    [Required(ErrorMessage = "user.email_required")]
    [EmailAddress(ErrorMessage = "user.invalid_email")]
    [MaxLength(255, ErrorMessage = "user.email_too_long")]
    string NewEmail,

    [Required(ErrorMessage = "user.password_required")]
    string Password
);
