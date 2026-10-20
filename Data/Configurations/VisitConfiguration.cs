using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("tb_visits");

        builder.HasKey(visit => visit.Id);

        builder.Property(visit => visit.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(visit => visit.VisitedOn)
            .IsRequired();

        builder.Property(visit => visit.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(visit => new { visit.UserId, visit.PlaceId, visit.VisitedOn })
            .IsUnique();

        builder.HasIndex(visit => new { visit.UserId, visit.VisitedOn });
        builder.HasIndex(visit => visit.PlaceId);

        builder.HasOne(visit => visit.User)
            .WithMany(user => user.Visits)
            .HasForeignKey(visit => visit.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(visit => visit.Place)
            .WithMany(place => place.Visits)
            .HasForeignKey(visit => visit.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
