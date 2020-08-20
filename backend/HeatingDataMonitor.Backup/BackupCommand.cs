using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliWrap;
using HeatingDataMonitor.Backup.Services;

namespace HeatingDataMonitor.Backup
{
    [Command("backup", Description = "Backup data from the api to a 7z archive as JSON.")]
    public class BackupCommand : ICommand
    {
        private readonly ISevenZipFactory _sevenZipFactory;

        public BackupCommand(ISevenZipFactory sevenZipFactory)
        {
            _sevenZipFactory = sevenZipFactory;
        }

        [CommandParameter(0, Description = "The path to the folder where the 7z archive should be created.")]
        public string ArchivePath { get; set; }

        [CommandOption("from", 'f', Description = "The time from where you want to start backing up data.", IsRequired = true)]
        public DateTime From { get; set; }

        [CommandOption("to", 't', Description = "The time you want to back up to. It'll also stop to archive once the api doesn't return any results.")]
        public DateTime To { get; set; } = DateTime.MaxValue;

        [CommandOption("endpoint", 'e', Description = "The Url to the desired api endpoint.")]
        public string Endpoint { get; set; } = "http://localhost/api/HeatingData";

        [CommandOption("interval", 'i', Description = "The size of the chunks that are queried from the api and added to the archive.")]
        public TimeSpan Interval { get; set; } = TimeSpan.FromDays(7);

        [CommandOption("7zip", Description = "The path to the 7-Zip executable used for adding to the archive.")]
        public string SevenZipPath { get; set; } = "7z.exe";

        public async ValueTask ExecuteAsync(IConsole console)
        {
            UriBuilder builder = new UriBuilder(Endpoint)
            {
                Query = FormatQuery()
            };
            // ValidateArchivePath end in .7z, doesn't matter if it exists

            using HttpClient httpClient = new HttpClient();
            while(true)
            {
                HttpResponseMessage response = await httpClient.GetAsync(builder.Uri);
                response.EnsureSuccessStatusCode();
                Stream stream = await response.Content.ReadAsStreamAsync();
                if (stream.CanSeek && stream.Length == 0)
                {
                    // how do you check if the api request returned anything?
                    break;
                }

                ISevenZip sevenZip = _sevenZipFactory.Create(SevenZipPath);
                sevenZip.AddToArchive(ArchivePath, GetFileName(), stream);
            }

            string FormatQuery() => $"from={From:o}&to={To:o}";
            string FormatFileName() => $"";
        }
    }
}
