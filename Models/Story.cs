using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class Story : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public string IntroText { get; set; } = string.Empty;
    public string? ClosingText { get; set; }
    public StoryOrigin Origin { get; set; } = StoryOrigin.Editorial;
    public StoryStatus Status { get; set; } = StoryStatus.Draft;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<StoryPlace> StoryPlaces { get; set; } = [];
}
