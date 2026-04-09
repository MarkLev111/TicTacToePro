using Microsoft.EntityFrameworkCore;
using TicTacToePro.Shared;

namespace TicTacToeProServer
{
    public class DBContext : DbContext
    {
        public DbSet<UserData> Users { get; set; } = null!;

        public DBContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserData>()
                .HasIndex(u => u.email)
                .IsUnique();

            builder.Entity<UserData>()
                .HasIndex(u => u.username)
                .IsUnique();
        }
    }
}
