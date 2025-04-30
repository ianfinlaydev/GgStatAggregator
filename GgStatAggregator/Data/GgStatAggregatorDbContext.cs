using static MudBlazor.CategoryTypes;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Table = GgStatAggregator.Models.Table;
using GgStatAggregator.Models;

namespace GgStatAggregator.Data
{
    public class GgStatAggregatorDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<StatSet> StatSets { get; set; }
        public DbSet<Table> Tables { get; set; }

        public GgStatAggregatorDbContext(DbContextOptions<GgStatAggregatorDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .Property(p => p.Id)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<StatSet>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Table>()
                .Property(t => t.Id)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Table>()
                .HasIndex(t => new { t.Stake, t.TableNumber })
                .IsUnique();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
