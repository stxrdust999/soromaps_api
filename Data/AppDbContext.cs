using Microsoft.EntityFrameworkCore;
using soromaps_api.Models;

namespace soromaps_api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<PlacePhoto> PlacePhotos => Set<PlacePhoto>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PlaceTag> PlaceTags => Set<PlaceTag>();
    public DbSet<ModerationDecision> ModerationDecisions => Set<ModerationDecision>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ReviewUsefulVote> ReviewUsefulVotes => Set<ReviewUsefulVote>();
    public DbSet<ContentRemoval> ContentRemovals => Set<ContentRemoval>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StoryPlace> StoryPlaces => Set<StoryPlace>();
    public DbSet<FeedMute> FeedMutes => Set<FeedMute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
