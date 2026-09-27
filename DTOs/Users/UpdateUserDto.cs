using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Users
{
    public record UpdateUserDto(
        [MaxLength(100)]
        string? Name,

        [MaxLength(500)]
        string? AvatarUrl,

        [MaxLength(280)]
        string? Biography,

        [MaxLength(60)]
        string? Neighborhood
    );
}
