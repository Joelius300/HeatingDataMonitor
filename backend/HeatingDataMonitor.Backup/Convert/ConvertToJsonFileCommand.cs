using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using HeatingDataMonitor.Model;

namespace HeatingDataMonitor.Backup.Convert
{
    [Command("convert toJsonFile", Description = "Converts a 7z-archive backup file to a single json file.")]
    public class ConvertToJsonFileCommand : ICommand
    {
        [CommandParameter(0, Description = ]
        public string Archive { get; set; }

        [CommandOption("output", 'o', Description = "The path to save the json file to.")]
        public string OutputPath { get; set; } = "{original-name}.json"; // adjust on runtime

        public ValueTask ExecuteAsync(IConsole console)
        {
            throw new NotImplementedException();
        }
    }
}
