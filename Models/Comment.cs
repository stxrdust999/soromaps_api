namespace soromaps_api.Models;

public class Comment : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
