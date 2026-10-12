using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;
using soromaps_api.Models.Enums;

namespace soromaps_api.Data.Configurations;

public class PlaceConfiguration : IEntityTypeConfiguration<Place>
{
    public void Configure(EntityTypeBuilder<Place> builder)
    {
        builder.ToTable("tb_places");

        builder.HasKey(place => place.Id);

        builder.Property(place => place.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(place => place.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(place => place.Latitude)
            .IsRequired();

        builder.Property(place => place.Longitude)
            .IsRequired();

        builder.Property(place => place.Neighborhood)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(place => place.About)
            .IsRequired()
            .HasMaxLength(280);

        builder.Property(place => place.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(place => place.HasWifi)
            .IsRequired();

        builder.Property(place => place.IsPetFriendly)
            .IsRequired();

        builder.Property(place => place.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(PlaceStatus.Pending);

        builder.Property(place => place.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(place => place.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(place => new { place.Status, place.CreatedAt });
        builder.HasIndex(place => place.CategoryId);
        builder.HasIndex(place => place.AuthorId);

        builder.HasOne(place => place.Category)
            .WithMany(category => category.Places)
            .HasForeignKey(place => place.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(place => place.Author)
            .WithMany(user => user.Places)
            .HasForeignKey(place => place.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
