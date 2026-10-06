using soromaps_api.DTOs.Users;

namespace soromaps_api.DTOs.Auth
{
    public record AuthResponseDto(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        UserResponseDto User
    )
    {
    }
}
