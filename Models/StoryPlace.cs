namespace soromaps_api.Models;

public class StoryPlace
{
    public Guid StoryId { get; set; }
    public Story Story { get; set; } = null!;

    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public short SortOrder { get; set; }
    public string AngleTitle { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string? LocalTip { get; set; }
}
