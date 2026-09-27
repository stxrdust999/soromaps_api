using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Users
{
    public record RegisterUserDto(
    [Required]
    [MaxLength(100)]
    string Name,

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    string Email,

    [Required]
    [MinLength(6)]
    [MaxLength(72)]
    string Password
    );
}
