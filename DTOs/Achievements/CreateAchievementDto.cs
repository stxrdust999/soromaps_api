using System.ComponentModel.DataAnnotations;

namespace soromaps_api.DTOs.Achievements;

public sealed record CreateAchievementDto(
    [Required(ErrorMessage = "achievement.name_required")]
    [MaxLength(80, ErrorMessage = "achievement.name_too_long")]
    string Name,

    [Required(ErrorMessage = "achievement.description_required")]
    [MaxLength(280, ErrorMessage = "achievement.description_too_long")]
    string Description
);
