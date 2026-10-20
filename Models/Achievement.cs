using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class Achievement : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "award";
    public string Color { get; set; } = "#F59E0B";
    public AchievementEventType EventType { get; set; } = AchievementEventType.Visit;
    public int RequiredQuantity { get; set; } = 1;
    public AchievementTargetType TargetType { get; set; } = AchievementTargetType.Any;
    public string? TargetValue { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = [];
}
