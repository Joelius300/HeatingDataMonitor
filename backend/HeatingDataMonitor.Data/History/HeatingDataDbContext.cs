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
    }
}
