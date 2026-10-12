using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("tb_comments");

        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(comment => comment.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(comment => comment.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(comment => comment.ReviewId);
        builder.HasIndex(comment => comment.UserId);

        builder.HasOne(comment => comment.Review)
            .WithMany(review => review.Comments)
            .HasForeignKey(comment => comment.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(comment => comment.User)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
