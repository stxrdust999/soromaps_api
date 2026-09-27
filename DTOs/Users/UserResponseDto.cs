using soromaps_api.Models;

namespace soromaps_api.DTOs.Users
{
    public record UserResponseDto(
        Guid Id,
        string Name,
        string Email,
        string Role,
        string? AvatarUrl,
        string? Biography,
        string? Neighborhood,
        DateTime CreatedAt
    )
    {
        public static UserResponseDto FromEntity(User user) =>
            new(
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.AvatarUrl,
                user.Biography,
                user.Neighborhood,
                user.CreatedAt
            );
    }
}
