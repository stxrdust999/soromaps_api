using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class FeedMuteConfiguration : IEntityTypeConfiguration<FeedMute>
{
    public void Configure(EntityTypeBuilder<FeedMute> builder)
    {
        builder.ToTable("tb_feed_mutes");

        builder.HasKey(fm => fm.Id);
        builder.Property(fm => fm.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(fm => fm.ScopeType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(fm => fm.ScopeValue)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(fm => fm.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(fm => fm.User)
            .WithMany(u => u.FeedMutes)
            .HasForeignKey(fm => fm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fm => new { fm.UserId, fm.ScopeType, fm.ScopeValue })
            .IsUnique();

        builder.HasIndex(fm => fm.UserId);
    }
}
