using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;

namespace HeatingDataMonitor.Backup
{
    [Command("backup", Description = "Backup data from the PostgreSQL database to a 7z archive as JSON.")]
    public class BackupCommand : ICommand
    {
        [CommandParameter(0, Description = "The PostgreSQL connection string for the HeatingData-datebase.")]
        public string ConnectionString { get; set; }

        [CommandParameter(1, Description = "The path to the folder where the 7z archive should be created.")]
        public string ArchivePath { get; set; }

        [CommandOption("from", Description = "The time from where you want to start backing up data.", IsRequired = true)]
        public DateTime From { get; set; }

        [CommandOption("to", Description = "The time you want to back up to.")]
        public DateTime To { get; set; } = DateTime.MaxValue;

        [CommandOption("interval", 'i', Description = "The size of the chunks that are queried from the api and added to the archive.")]
        public TimeSpan Interval { get; set; } = TimeSpan.FromDays(7);

        [CommandOption("7zip", Description = "The path to the 7-Zip executable used for adding to the archive.")]
        public string SevenZipPath { get; set; } = "7z.exe";

        [CommandOption("endpoint", 'e', Description = )] = "http://localhost/api/HeatingData";
        public string Endpoint { get; set; }

        public ValueTask ExecuteAsync(IConsole console)
        {
            UriBuilder builder = new UriBuilder(Endpoint)
            {
                Query = $"from={From:o}&to={To:o}"
            };
            // ValidateArchivePath end in .7z, doesn't matter if it exists

        }
    }
}
