using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Achievements;

public sealed record UpdateAchievementDto(
    [MaxLength(80, ErrorMessage = "achievement.name_too_long")]
    string? Name,

    [MaxLength(280, ErrorMessage = "achievement.description_too_long")]
    string? Description
);
