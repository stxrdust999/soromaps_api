namespace soromaps_api.DTOs.Users;

public sealed record UserResponseDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string? AvatarUrl,
    string? Biography,
    string? Neighborhood,
    DateTime CreatedAt
);
