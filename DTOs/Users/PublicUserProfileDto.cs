namespace soromaps_api.DTOs.Users;

public sealed record PublicUserProfileDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    string? Biography,
    string? Neighborhood,
    DateTime CreatedAt
);
