using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("tb_reviews", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("ck_tb_reviews_rating", "rating >= 1 AND rating <= 5");
        });

        builder.HasKey(review => review.Id);

        builder.Property(review => review.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(review => review.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(review => review.Rating)
            .IsRequired();

        builder.Property(review => review.RemovedAt);

        builder.Property(review => review.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(review => review.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(review => new { review.PlaceId, review.UserId })
            .IsUnique()
            .HasFilter("removed_at IS NULL");

        builder.HasIndex(review => new { review.PlaceId, review.CreatedAt });

        builder.HasOne(review => review.Place)
            .WithMany(place => place.Reviews)
            .HasForeignKey(review => review.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(review => review.User)
            .WithMany(user => user.Reviews)
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
