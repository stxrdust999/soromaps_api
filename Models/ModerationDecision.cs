using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class ModerationDecision
{
    public Guid Id { get; set; }

    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public Guid? ModeratorId { get; set; }
    public User? Moderator { get; set; }

    public PlaceStatus PreviousStatus { get; set; }
    public PlaceStatus Decision { get; set; }
    public string? Reason { get; set; }
    public string? FeedbackNote { get; set; }
    public bool IsUndo { get; set; }
    public DateTime CreatedAt { get; set; }
}
