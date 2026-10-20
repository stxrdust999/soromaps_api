using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;
using soromaps_api.Models.Enums;

namespace soromaps_api.Data.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("tb_achievements");

        builder.HasKey(achievement => achievement.Id);

        builder.Property(achievement => achievement.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(achievement => achievement.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(achievement => achievement.Description)
            .IsRequired()
            .HasMaxLength(280);

        builder.Property(achievement => achievement.Icon)
            .IsRequired()
            .HasMaxLength(40)
            .HasDefaultValue("award");

        builder.Property(achievement => achievement.Color)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("#F59E0B");

        builder.Property(achievement => achievement.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(AchievementEventType.Visit);

        builder.Property(achievement => achievement.RequiredQuantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(achievement => achievement.TargetType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(AchievementTargetType.Any);

        builder.Property(achievement => achievement.TargetValue)
            .HasMaxLength(60);

        builder.Property(achievement => achievement.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(achievement => achievement.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(achievement => achievement.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(achievement => achievement.Name)
            .IsUnique();

        builder.HasIndex(achievement => new { achievement.IsActive, achievement.EventType });
    }
}
