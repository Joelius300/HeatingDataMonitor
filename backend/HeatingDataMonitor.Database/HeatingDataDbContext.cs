using HeatingDataMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace HeatingDataMonitor.Database;

public class HeatingDataDbContext : DbContext
{
    public DbSet<HeatingData> HeatingData { get; set; } = null!;

    public HeatingDataDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // There's no real PK afaik but ReceivedTime should always be unique
        // and in order to insert a new record, EF core needs a key assigned.
        modelBuilder.Entity<HeatingData>().HasKey(e => e.ReceivedTime);
    }
}
