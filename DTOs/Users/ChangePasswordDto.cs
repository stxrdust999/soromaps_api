using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Users;

public sealed record ChangePasswordDto(
    [Required(ErrorMessage = "user.current_password_required")]
    string CurrentPassword,

    [Required(ErrorMessage = "user.new_password_required")]
    [MinLength(8, ErrorMessage = "user.password_too_short")]
    [MaxLength(100, ErrorMessage = "user.password_too_long")]
    string NewPassword,

    [Required(ErrorMessage = "user.confirm_password_required")]
    [property: Compare("NewPassword", ErrorMessage = "user.passwords_do_not_match")]
    string ConfirmPassword
);
