using CliFx;
using CliFx.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HeatingDataMonitor.Backup.Convert
{
    [Command("convert toCsvFile", Description = "Converts a 7z-archive backup file to a single csv file.")]
    public class ConvertToCsvFileCommand : ICommand
    {
        // Maybe option for formatting it like the SPS would?

        public ValueTask ExecuteAsync(IConsole console)
        {
            /* Something Utf8JsonReader
             * (maybe https://github.com/evil-dr-nick/utf8jsonstreamreader, maybe extracting and then going file by file is enough);
             * Something Subclass of HeatingDataCsvMap;
             */

            throw new NotImplementedException();
        }
    }
}
