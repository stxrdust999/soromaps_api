using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class ContentRemovalConfiguration : IEntityTypeConfiguration<ContentRemoval>
{
    public void Configure(EntityTypeBuilder<ContentRemoval> builder)
    {
        builder.ToTable("tb_content_removals");

        builder.HasKey(removal => removal.Id);

        builder.Property(removal => removal.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(removal => removal.TargetType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(removal => removal.TargetId)
            .IsRequired();

        builder.Property(removal => removal.Reason)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(removal => removal.Note)
            .HasMaxLength(500);

        builder.Property(removal => removal.SnapshotText)
            .IsRequired()
            .HasMaxLength(3000);

        builder.Property(removal => removal.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(removal => new { removal.TargetType, removal.TargetId });
        builder.HasIndex(removal => removal.AuthorId);
        builder.HasIndex(removal => removal.ModeratorId);
        builder.HasIndex(removal => removal.CreatedAt);

        builder.HasOne(removal => removal.Author)
            .WithMany()
            .HasForeignKey(removal => removal.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(removal => removal.Moderator)
            .WithMany()
            .HasForeignKey(removal => removal.ModeratorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
