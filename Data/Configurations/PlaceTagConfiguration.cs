using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class PlaceTagConfiguration : IEntityTypeConfiguration<PlaceTag>
{
    public void Configure(EntityTypeBuilder<PlaceTag> builder)
    {
        builder.ToTable("tb_place_tags");

        builder.HasKey(placeTag => new { placeTag.PlaceId, placeTag.TagId });

        builder.HasIndex(placeTag => placeTag.TagId);

        builder.HasOne(placeTag => placeTag.Place)
            .WithMany(place => place.PlaceTags)
            .HasForeignKey(placeTag => placeTag.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(placeTag => placeTag.Tag)
            .WithMany(tag => tag.PlaceTags)
            .HasForeignKey(placeTag => placeTag.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
