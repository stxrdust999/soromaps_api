using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(user => user.Id);

            builder.Property(user => user.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(user => user.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(user => user.Email)
                .IsUnique();

            builder.Property(user => user.PasswordHash)
                .IsRequired();

            builder.Property(user => user.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("explorer");

            builder.Property(user => user.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(user => user.Biography)
                .HasMaxLength(280);

            builder.Property(user => user.Neighborhood)
                .HasMaxLength(60);

            builder.Property(user => user.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.Property(user => user.UpdatedAt)
                .HasDefaultValueSql("now()");
        }
    }
}
