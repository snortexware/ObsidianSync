using G.Sync.Entities;
using Microsoft.EntityFrameworkCore;

namespace G.Sync.Repository
{
    public class GSyncContext : DbContext
    {
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<SettingsEntity> Settings { get; set; }
        public DbSet<SecurityEntity> Securities { get; set; }

        private const string dbPath = @"C:\obsidian-sync\sync.db";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={dbPath};Pooling=true;");
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
        }
    }
}
