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
    public DbSet<Achievement> Achievements => Set<Achievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
