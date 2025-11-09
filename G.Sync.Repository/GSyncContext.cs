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

        private const string dbPath = @"C:\obsidian-sync\sync.db";

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
