using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Auth
{
    public record LoginRequestDto(
        [Required(ErrorMessage = "auth.email_required")]
        [EmailAddress(ErrorMessage = "auth.invalid_email")]
        string Email,

        [Required(ErrorMessage = "auth.password_required")]
        [MaxLength(72, ErrorMessage = "auth.password_too_long")]
        string Password
    )
    {

    }
}
