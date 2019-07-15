using DataHandler;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataHistory
{
    public class HeatingDataContext : DbContext
    {
        public DbSet<Data> Data { get; set; }
    }
}
