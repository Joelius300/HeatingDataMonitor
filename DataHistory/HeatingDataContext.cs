using DataHandler;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataHistory
{
    public class HeatingDataContext : DbContext
    {
        public static HeatingDataContext Instance { get; set; }

        private readonly string connectionString;

        public DbSet<Data> Data { get; set; }

        public HeatingDataContext(string connectionString) => this.connectionString = connectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
