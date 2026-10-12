using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

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

        builder.HasIndex(achievement => achievement.Name)
            .IsUnique();

        builder.Property(achievement => achievement.Description)
            .IsRequired()
            .HasMaxLength(280);

        builder.Property(achievement => achievement.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(achievement => achievement.UpdatedAt)
            .HasDefaultValueSql("now()");
    }
}
