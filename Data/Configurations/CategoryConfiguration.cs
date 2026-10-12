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

        builder.HasIndex(category => category.Name)
            .IsUnique();

        builder.Property(category => category.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(category => category.UpdatedAt)
            .HasDefaultValueSql("now()");
    }
}
