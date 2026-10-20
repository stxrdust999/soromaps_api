using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class StoryPlaceConfiguration : IEntityTypeConfiguration<StoryPlace>
{
    public void Configure(EntityTypeBuilder<StoryPlace> builder)
    {
        builder.ToTable("tb_story_places");

        builder.HasKey(sp => new { sp.StoryId, sp.PlaceId });

        builder.Property(sp => sp.SortOrder)
            .IsRequired();

        builder.Property(sp => sp.AngleTitle)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(sp => sp.BodyText)
            .IsRequired();

        builder.Property(sp => sp.LocalTip)
            .HasMaxLength(280);

        builder.HasOne(sp => sp.Story)
            .WithMany(s => s.StoryPlaces)
            .HasForeignKey(sp => sp.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Place)
            .WithMany(p => p.StoryPlaces)
            .HasForeignKey(sp => sp.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sp => sp.PlaceId);
    }
}
