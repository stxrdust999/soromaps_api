namespace soromaps_api.Models;

public class PlaceTag
{
    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
