using HeatingDataMonitor.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeatingDataMonitor.History
{
    public class HeatingDataDbContext : DbContext
    {
        public DbSet<HeatingData> HeatingData { get; set; }

        public HeatingDataDbContext(DbContextOptions options) : base(options)
        {
        }

        protected HeatingDataDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Pretty much all queries will be made against the time so having an index for that
            // should speed up the queries right? Maybe investigate that further because on queries
            // that return a lot of rows (like mosts of ours), an index might slow things down.
            modelBuilder
                .Entity<HeatingData>()
                .HasIndex(d => d.Zeit);
        }
    }
}
