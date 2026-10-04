using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Auth
{
    public record RefreshTokenRequestDto(
        [Required(ErrorMessage = "auth.refresh_token_required")]
        string RefreshToken
    )
    {
    }
}
