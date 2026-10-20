using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class ReviewUsefulVoteConfiguration : IEntityTypeConfiguration<ReviewUsefulVote>
{
    public void Configure(EntityTypeBuilder<ReviewUsefulVote> builder)
    {
        builder.ToTable("tb_review_useful_votes");

        builder.HasKey(vote => new { vote.ReviewId, vote.UserId });

        builder.Property(vote => vote.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(vote => vote.UserId);

        builder.HasOne(vote => vote.Review)
            .WithMany(review => review.UsefulVotes)
            .HasForeignKey(vote => vote.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vote => vote.User)
            .WithMany(user => user.UsefulVotes)
            .HasForeignKey(vote => vote.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
