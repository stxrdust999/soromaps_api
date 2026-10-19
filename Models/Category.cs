namespace soromaps_api.Models;

public class Category : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Icon { get; set; } = "map-pin";
    public string Color { get; set; } = "#2563EB";
    public short SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Place> Places { get; set; } = [];
}
