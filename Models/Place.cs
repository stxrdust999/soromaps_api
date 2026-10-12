using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class Place : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }

    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool HasWifi { get; set; }
    public bool IsPetFriendly { get; set; }
    public PlaceStatus Status { get; set; } = PlaceStatus.Pending;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Review> Reviews { get; set; } = [];
}
