using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataHistory
{
    public class HistoryServiceOptions
    {
        [Required]
        public string SQLiteConnectionString { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 1000)]
        public int SaveIntervalInMinutes { get; set; }
    }
}
