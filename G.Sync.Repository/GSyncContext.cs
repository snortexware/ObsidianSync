using G.Sync.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace G.Sync.Repository
{
    public class GSyncContext : DbContext
    {
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<SettingsEntity> Settings { get; set; }
        public DbSet<SecurityEntity> Securities { get; set; }
        public DbSet<TaskQueue> TaskQueues { get; set; }
        public DbSet<VaultsEntity> Vaults { get; set; }

        private readonly string dbPath = GetDatabasePath();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            }.ToString();

            var connection = new SqliteConnection(connectionString);
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "PRAGMA busy_timeout = 5000;";
                cmd.ExecuteNonQuery();
            }

            optionsBuilder.UseSqlite(connection);
        }

        private static string GetDatabasePath()
        {

            string appDataPath = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ObsidianSync"),
                PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ObsidianSync"),
                _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ObsidianSync")
            };

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return Path.Combine(appDataPath, "sync.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);
            });

            modelBuilder.Entity<SecurityEntity>(entity => { entity.HasKey(e => e.Id); });

            modelBuilder.Entity<SettingsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<VaultsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
