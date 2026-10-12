namespace soromaps_api.Models;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
