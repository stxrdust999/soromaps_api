using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tb_tags");

        builder.HasKey(tag => tag.Id);

        builder.Property(tag => tag.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(tag => tag.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tag => tag.Slug)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(tag => tag.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.HasIndex(tag => tag.Name)
            .IsUnique();

        builder.HasIndex(tag => tag.Slug)
            .IsUnique();
    }
}
