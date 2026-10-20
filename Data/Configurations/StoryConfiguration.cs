using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable("tb_stories");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(s => s.Title)
            .HasMaxLength(140)
            .IsRequired();

        builder.Property(s => s.Subtitle)
            .HasMaxLength(280)
            .IsRequired();

        builder.Property(s => s.Neighborhood)
            .HasMaxLength(60);

        builder.Property(s => s.IntroText)
            .IsRequired();

        builder.Property(s => s.Origin)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(s => s.Author)
            .WithMany(u => u.AuthoredStories)
            .HasForeignKey(s => s.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.Slug)
            .IsUnique();

        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.PublishedAt);
    }
}
