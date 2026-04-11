using Microsoft.EntityFrameworkCore;
using TicTacToePro.Shared;

namespace TicTacToeProServer
{
    public class DBContext : DbContext
    {
        public DbSet<UserData> Users { get; set; } = null!;
        public DbSet<MultiplayerStats> Stats { get; set; } = null!;

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

            builder.Entity<UserData>()
                .HasOne(u => u.stats)          // У юзера есть одна статистика
                .WithOne()                     // У статистики есть один юзер (обратная ссылка не обязательна)
                .HasForeignKey<MultiplayerStats>("UserId") // В таблице Stats появится колонка UserId
                .OnDelete(DeleteBehavior.Cascade);         // Удалили юзера -> удалилась стата
        }
    }
}
