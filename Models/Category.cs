namespace soromaps_api.Models;

public class Category : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Place> Places { get; set; } = [];
}
