namespace soromaps_api.Models;

public class Favorite
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
