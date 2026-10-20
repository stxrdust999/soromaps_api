using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class Feedback
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public FeedbackCategory Category { get; set; }
    public string? OriginPath { get; set; }
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
