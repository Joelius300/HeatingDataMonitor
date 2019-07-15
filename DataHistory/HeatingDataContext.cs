using DataHandler;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataHistory
{
    public class HeatingDataContext : DbContext
    {
        public HeatingDataContext(DbContextOptions options) : base(options) { }

        public DbSet<Data> Data { get; set; }
    }
}
