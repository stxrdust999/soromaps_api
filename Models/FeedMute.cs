using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class FeedMute
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public FeedMuteScopeType ScopeType { get; set; }
    public string ScopeValue { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
