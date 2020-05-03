using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace CsvParsingFromStreamDemo
{
    class ParseFromMemoryStream : IDisposable
    {
        MemoryStream buffer;
        StreamReader bufferReader;
        CsvReader reader;
        CancellationTokenSource cts;
        AutoResetEvent newDataEvent = new AutoResetEvent(false);

        public void Start()
        {
            cts = new CancellationTokenSource();
            buffer = new MemoryStream();
            bufferReader = new StreamReader(buffer, Encoding.ASCII);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                NewLine = NewLine.CRLF,
                IgnoreBlankLines = true,
                IgnoreQuotes = true,
                Encoding = Encoding.ASCII,
                HasHeaderRecord = false
            };

            reader = new CsvReader(bufferReader, csvConfig);
            Thread th = new Thread(Read);
            th.Start();

            while (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("Enter Data to add to the buffer as ASCII bytes. \\r and \\n will be translated.");
                string newData = Console.ReadLine();
                if (newData.Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    Stop();
                    return;
                }
                else if (newData.Equals("print", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(bufferReader.ReadToEnd());
                    Stop();
                    return;
                }

                newData = newData.Replace("\\r", "\r").Replace("\\n", "\n");

                byte[] bytes = Encoding.ASCII.GetBytes(newData);
                buffer.Write(bytes, 0, bytes.Length); // we could also use a StreamWriter but this is closer to the SP
                newDataEvent.Set();
            }
        }

        public void Stop()
        {
            cts.Cancel();
            newDataEvent.Set();
        }

        private void Read()
        {
            // int readIndex = 0;
            CancellationToken ct = cts.Token;
            while (!ct.IsCancellationRequested)
            {
                Console.WriteLine("Waiting for new Data");
                if (!newDataEvent.WaitOne(2500))
                {
                    Console.WriteLine("No data was added in the given interval");
                    continue;
                }

                if (ct.IsCancellationRequested)
                    return;

                if (reader.Read())
                {
                    Data parsed = reader.GetRecord<Data>();
                    Console.WriteLine($"Parsed data: {parsed}");
                }
                else
                {
                    Console.WriteLine("reader.Read() returned false");
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Stop();
                    cts.Dispose();
                    reader.Dispose();
                    buffer.Dispose();
                    bufferReader.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ParseFromMemoryStream()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
