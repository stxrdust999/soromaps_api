namespace soromaps_api.Models;

public class ContentRemoval
{
    public Guid Id { get; set; }
    public string TargetType { get; set; } = string.Empty; // "review" | "comment"
    public Guid TargetId { get; set; }
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
    public Guid? ModeratorId { get; set; }
    public User? Moderator { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string SnapshotText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
