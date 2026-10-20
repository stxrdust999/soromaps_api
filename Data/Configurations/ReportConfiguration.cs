using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("tb_reports");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.TargetType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.TargetId)
            .IsRequired();

        builder.Property(r => r.Reason)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(r => r.Details)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.ResolutionNote)
            .HasMaxLength(300);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(r => r.Reporter)
            .WithMany(u => u.FiledReports)
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ResolvedBy)
            .WithMany(u => u.ResolvedReports)
            .HasForeignKey(r => r.ResolvedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.ReporterId, r.TargetType, r.TargetId })
            .IsUnique();

        builder.HasIndex(r => new { r.TargetType, r.TargetId });
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);
    }
}
