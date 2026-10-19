using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("tb_categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(category => category.Slug)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(category => category.Icon)
            .IsRequired()
            .HasMaxLength(40)
            .HasDefaultValue("map-pin");

        builder.Property(category => category.Color)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#2563EB");

        builder.Property(category => category.SortOrder)
            .IsRequired()
            .HasDefaultValue((short)0);

        builder.Property(category => category.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(category => category.Name)
            .IsUnique();

        builder.HasIndex(category => category.Slug)
            .IsUnique();

        builder.HasIndex(category => new { category.IsActive, category.SortOrder });

        builder.Property(category => category.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(category => category.UpdatedAt)
            .HasDefaultValueSql("now()");
    }
}
