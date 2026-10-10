using soromaps_api.Models;

namespace soromaps_api.DTOs
{
    public record AchievementResponseDto(
         Guid AchievementId,
         string AchievementName,
         string AchievementDesc
     )
    {
        public static AchievementResponseDto FromEntity(Achievement achievement) =>
            new(
                achievement.AchievementId,
                achievement.AchievementName,
                achievement.AchievementDesc
            );
    }

    public record CreateAchievementDto(
        string AchievementName,
        string AchievementDesc
    );

    public record UpdateAchievementDto(
        string? AchievementName,
        string? AchievementDesc
    );
}
