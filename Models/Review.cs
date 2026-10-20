namespace soromaps_api.Models;

public class Review : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime? RemovedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<ReviewUsefulVote> UsefulVotes { get; set; } = [];
}
