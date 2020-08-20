using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HeatingDataMonitor.Backup.Services
{
    public interface ISevenZip
    {
        Task AddToArchive(string path, string fileName, Stream content);
    }
}
