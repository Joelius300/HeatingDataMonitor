using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace HeatingDataMonitor.Backup
{
    [Command("restore", Description = "Restores data from a 7z archive to the PostgreSQL database.")]
    public class RestoreCommand : ICommand
    {
        [CommandParameter(0, Description = "The PostgreSQL connection string for the HeatingData-datebase.")]
        public string ConnectionString { get; set; }

        [CommandParameter(1, Description = "The path to the 7z archive which should be restored.")]
        public string ArchivePath { get; set; }
        
        public ValueTask ExecuteAsync(IConsole console)
        {
            throw new NotImplementedException();
        }
    }
}
