
using CliWrap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HeatingDataMonitor.Backup.Services
{
    public class SevenZip : ISevenZip
    {
        private readonly string _executablePath;

        public SevenZip(string executablePath)
        {
            if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
                throw new ArgumentException(TODO);

            _executablePath = executablePath;
        }

        public async Task AddToArchive(string path, string fileName, Stream content)
        {
            await Cli.Wrap(_executablePath)
               .WithArguments()
               .WithStandardInputPipe(PipeSource.FromStream(content))
               .WithValidation(CommandResultValidation.ZeroExitCode)
               .ExecuteAsync();
        }
    }
}
