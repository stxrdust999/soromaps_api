using System.Linq.Expressions;
using soromaps_api.DTOs.Achievements;
using soromaps_api.Models;

namespace soromaps_api.Mappings;

public static class AchievementMappings
{
    public static Expression<Func<Achievement, AchievementResponseDto>> ToResponseProjection(int totalExplorersCount) =>
        achievement => new AchievementResponseDto(
            achievement.Id,
            achievement.Name,
            achievement.Description,
            achievement.Icon,
            achievement.Color,
            achievement.EventType,
            achievement.RequiredQuantity,
            achievement.TargetType,
            achievement.TargetValue,
            achievement.IsActive,
            achievement.UserAchievements.Count,
            totalExplorersCount > 0
                ? Math.Round((double)achievement.UserAchievements.Count / totalExplorersCount * 100.0, 1)
                : 0.0,
            achievement.CreatedAt,
            achievement.UpdatedAt
        );

    public static AchievementResponseDto ToResponse(this Achievement achievement, int holdersCount = 0, double holdersPercentage = 0.0) =>
        new(
            achievement.Id,
            achievement.Name,
            achievement.Description,
            achievement.Icon,
            achievement.Color,
            achievement.EventType,
            achievement.RequiredQuantity,
            achievement.TargetType,
            achievement.TargetValue,
            achievement.IsActive,
            holdersCount,
            holdersPercentage,
            achievement.CreatedAt,
            achievement.UpdatedAt
        );

    public static void ApplyUpdate(this Achievement achievement, UpdateAchievementDto request)
    {
        if (request.Name is not null)
            achievement.Name = request.Name.Trim();

        if (request.Description is not null)
            achievement.Description = request.Description.Trim();

        if (request.Icon is not null)
            achievement.Icon = request.Icon.Trim();

        if (request.Color is not null)
            achievement.Color = request.Color.Trim();

        if (request.EventType.HasValue)
            achievement.EventType = request.EventType.Value;

        if (request.RequiredQuantity.HasValue)
            achievement.RequiredQuantity = request.RequiredQuantity.Value;

        if (request.TargetType.HasValue)
            achievement.TargetType = request.TargetType.Value;

        if (request.TargetValue is not null)
            achievement.TargetValue = string.IsNullOrWhiteSpace(request.TargetValue) ? null : request.TargetValue.Trim();

        if (request.IsActive.HasValue)
            achievement.IsActive = request.IsActive.Value;
    }
}
