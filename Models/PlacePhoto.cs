namespace soromaps_api.Models;

public class PlacePhoto
{
    public Guid Id { get; set; }

    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public Guid? UploadedById { get; set; }
    public User? UploadedBy { get; set; }

    public string Url { get; set; } = string.Empty;
    public string? StoragePath { get; set; }
    public bool IsCover { get; set; }
    public DateTime CreatedAt { get; set; }
}
