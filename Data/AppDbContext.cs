using Microsoft.EntityFrameworkCore;
using soromaps_api.Models;

namespace soromaps_api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Place> Places => Set<Place>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Comment> Comments => Set <Comment>();
        public DbSet<Achievement> Achievements => Set <Achievement>();
        public DbSet<Category> Categorys => Set <Category>();

        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();

                if (!string.IsNullOrEmpty(tableName) && !tableName.StartsWith("tb_"))
                {
                    entity.SetTableName($"tb_{tableName}");
                }
            }
        }
    }
}
