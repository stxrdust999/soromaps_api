namespace soromaps_api.DTOs.Achievements;

public sealed record AchievementResponseDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
