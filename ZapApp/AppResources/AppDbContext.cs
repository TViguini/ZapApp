using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ZapApp.AppResources
{
    public class AppDbContext : DbContext
    {
        public DbSet<Registro> Registros { get; set; }
        public DbSet<Path_PDF> Path_PDF { get; set; }

        private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "zapapp.db");

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
