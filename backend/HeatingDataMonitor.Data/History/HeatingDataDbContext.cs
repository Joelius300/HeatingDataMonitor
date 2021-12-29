using HeatingDataMonitor.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace HeatingDataMonitor.Data.History;

public class HeatingDataDbContext : DbContext
{
    public DbSet<HeatingData> HeatingData { get; set; } = null!;

    public HeatingDataDbContext(DbContextOptions options) : base(options)
    {
    }
}
