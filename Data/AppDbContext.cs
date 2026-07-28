using Microsoft.EntityFrameworkCore;
using Soromaps.Models;

namespace Soromaps.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Marker> Markers { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
