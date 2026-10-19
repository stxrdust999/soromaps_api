using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class ModerationDecisionConfiguration : IEntityTypeConfiguration<ModerationDecision>
{
    public void Configure(EntityTypeBuilder<ModerationDecision> builder)
    {
        builder.ToTable("tb_moderation_decisions");

        builder.HasKey(decision => decision.Id);

        builder.Property(decision => decision.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(decision => decision.PreviousStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(decision => decision.Decision)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(decision => decision.Reason)
            .HasMaxLength(50);

        builder.Property(decision => decision.FeedbackNote)
            .HasMaxLength(500);

        builder.Property(decision => decision.IsUndo)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(decision => decision.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(decision => new { decision.PlaceId, decision.CreatedAt });
        builder.HasIndex(decision => decision.CreatedAt);

        builder.HasOne(decision => decision.Place)
            .WithMany(place => place.ModerationDecisions)
            .HasForeignKey(decision => decision.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(decision => decision.Moderator)
            .WithMany(user => user.ModerationDecisions)
            .HasForeignKey(decision => decision.ModeratorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
