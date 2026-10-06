using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using soromaps_api.Models;

namespace soromaps_api.Data.Configurations
{
    public class SessionConfiguration: IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(session => session.Id);

            builder.Property(session => session.TokenHash)
                .IsRequired()
                .HasMaxLength(64)
                .IsFixedLength();

            builder.HasIndex(session => session.TokenHash)
                .IsUnique();

            builder.Property(session => session.UserAgent)
                .HasMaxLength(300);

            builder.Property(session => session.ExpiresAt)
                .IsRequired();

            builder.Property(session => session.CreatedAt)
                .HasDefaultValueSql("now()");

            builder.HasOne(session => session.User)
                .WithMany(user => user.Sessions)
                .HasForeignKey(session => session.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
