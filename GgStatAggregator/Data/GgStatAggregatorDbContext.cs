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
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<Table>()
                .HasIndex(t => new { t.Stake, t.TableNumber })
                .IsUnique();
        }
    }
}
