namespace soromaps_api.Models;

public class Visit
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public DateOnly VisitedOn { get; set; }
    public DateTime CreatedAt { get; set; }
}
