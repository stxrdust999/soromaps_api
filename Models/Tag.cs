namespace soromaps_api.Models;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<PlaceTag> PlaceTags { get; set; } = [];
}
