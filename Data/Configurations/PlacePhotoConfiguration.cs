using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class PlacePhotoConfiguration : IEntityTypeConfiguration<PlacePhoto>
{
    public void Configure(EntityTypeBuilder<PlacePhoto> builder)
    {
        builder.ToTable("tb_place_photos");

        builder.HasKey(photo => photo.Id);

        builder.Property(photo => photo.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(photo => photo.Url)
            .IsRequired()
            .HasMaxLength(600);

        builder.Property(photo => photo.StoragePath)
            .HasMaxLength(300);

        builder.Property(photo => photo.IsCover)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(photo => photo.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(photo => photo.PlaceId);

        builder.HasIndex(photo => photo.PlaceId)
            .IsUnique()
            .HasFilter("is_cover = true");

        builder.HasOne(photo => photo.Place)
            .WithMany(place => place.Photos)
            .HasForeignKey(photo => photo.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(photo => photo.UploadedBy)
            .WithMany(user => user.UploadedPlacePhotos)
            .HasForeignKey(photo => photo.UploadedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
