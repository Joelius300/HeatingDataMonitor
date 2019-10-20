using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace DataHandler
{
    public class HeatingMonitorOptions
    {
        [DefaultValue(false)]
        public bool DebugMode { get; set; } = false;
        public string SerialPortName { get; set; } = string.Empty;
        [Range(1, 1000)]
        [Required]
        public int ExpectedReadIntervalInSeconds { get; set; }
    }
}
