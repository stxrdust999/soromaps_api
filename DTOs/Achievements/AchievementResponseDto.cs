using System.ComponentModel.DataAnnotations;
using soromaps_api.Models.Enums;

namespace soromaps_api.DTOs.Achievements;

public sealed record AchievementResponseDto(
    Guid Id,
    string Name,
    string Description,
    string Icon,
    string Color,
    AchievementEventType EventType,
    int RequiredQuantity,
    AchievementTargetType TargetType,
    string? TargetValue,
    bool IsActive,
    int HoldersCount,
    double HoldersPercentage,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateAchievementDto(
    [Required(ErrorMessage = "achievement.name_required")]
    [MaxLength(80, ErrorMessage = "achievement.name_too_long")]
    string Name,

    [Required(ErrorMessage = "achievement.description_required")]
    [MaxLength(280, ErrorMessage = "achievement.description_too_long")]
    string Description,

    [MaxLength(40, ErrorMessage = "achievement.icon_too_long")]
    string? Icon = "award",

    [MaxLength(20, ErrorMessage = "achievement.color_too_long")]
    string? Color = "#F59E0B",

    AchievementEventType EventType = AchievementEventType.Visit,

    [Range(1, 1000, ErrorMessage = "achievement.invalid_quantity")]
    int RequiredQuantity = 1,

    AchievementTargetType TargetType = AchievementTargetType.Any,

    [MaxLength(60, ErrorMessage = "achievement.target_value_too_long")]
    string? TargetValue = null,

    bool IsActive = true
);

public sealed record UpdateAchievementDto(
    [MaxLength(80, ErrorMessage = "achievement.name_too_long")]
    string? Name = null,

    [MaxLength(280, ErrorMessage = "achievement.description_too_long")]
    string? Description = null,

    [MaxLength(40, ErrorMessage = "achievement.icon_too_long")]
    string? Icon = null,

    [MaxLength(20, ErrorMessage = "achievement.color_too_long")]
    string? Color = null,

    AchievementEventType? EventType = null,

    [Range(1, 1000, ErrorMessage = "achievement.invalid_quantity")]
    int? RequiredQuantity = null,

    AchievementTargetType? TargetType = null,

    [MaxLength(60, ErrorMessage = "achievement.target_value_too_long")]
    string? TargetValue = null,

    bool? IsActive = null
);

public sealed record UserAchievementProgressDto(
    Guid Id,
    string Name,
    string Description,
    string Icon,
    string Color,
    int CurrentProgress,
    int RequiredQuantity,
    bool IsUnlocked,
    DateTime? UnlockedAt
);

public sealed record AchievementEstimateQueryDto(
    AchievementEventType EventType,
    int RequiredQuantity,
    AchievementTargetType TargetType = AchievementTargetType.Any,
    string? TargetValue = null
);

public sealed record AchievementEstimateResponseDto(
    int TotalExplorersCount,
    int EstimatedEligibleCount,
    double EstimatedPercentage
);
