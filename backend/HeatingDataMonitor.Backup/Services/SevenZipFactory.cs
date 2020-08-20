using System;
using System.Collections.Generic;
using System.Text;

namespace HeatingDataMonitor.Backup.Services
{
    public class SevenZipFactory : ISevenZipFactory
    {
        public ISevenZip Create(string executablePath) => new SevenZip(executablePath);
    }
}
