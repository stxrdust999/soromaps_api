using soromaps_api.Models.Enums;

namespace soromaps_api.Models;

public class User : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Explorer;
    public string? AvatarUrl { get; set; }
    public string? Biography { get; set; }
    public string? Neighborhood { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Session> Sessions { get; set; } = [];
    public ICollection<Place> Places { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<PlacePhoto> UploadedPlacePhotos { get; set; } = [];
    public ICollection<ModerationDecision> ModerationDecisions { get; set; } = [];
    public ICollection<ReviewUsefulVote> UsefulVotes { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
    public ICollection<UserAchievement> UserAchievements { get; set; } = [];
}
