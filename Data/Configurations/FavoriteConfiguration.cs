using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("tb_favorites");

        builder.HasKey(favorite => new { favorite.UserId, favorite.PlaceId });

        builder.Property(favorite => favorite.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(favorite => favorite.PlaceId);

        builder.HasOne(favorite => favorite.User)
            .WithMany(user => user.Favorites)
            .HasForeignKey(favorite => favorite.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(favorite => favorite.Place)
            .WithMany(place => place.Favorites)
            .HasForeignKey(favorite => favorite.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
