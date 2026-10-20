using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("tb_user_achievements");

        builder.HasKey(ua => new { ua.UserId, ua.AchievementId });

        builder.Property(ua => ua.UnlockedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(ua => ua.AchievementId);

        builder.HasOne(ua => ua.User)
            .WithMany(user => user.UserAchievements)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Achievement)
            .WithMany(achievement => achievement.UserAchievements)
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
