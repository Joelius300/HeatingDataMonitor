using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace CsvParsingFromStreamDemo
{
    public class ReadFromPortIdea1 : IDisposable
    {
        private readonly SerialPort _port;
        private readonly Thread _readingThread;
        private readonly Thread _processingThread;
        private readonly AutoResetEvent _syncEvent;
        private readonly CancellationTokenSource _cts;
        private readonly MemoryStream _bufferStream;
        private readonly StreamReader _reader;
        private long _readingIndex;

        public ReadFromPortIdea1(string name)
        {
            _port = new SerialPort()
            {
                PortName = name,
                BaudRate = 9600,
                DataBits = 8,
                Parity = Parity.None,
                Handshake = Handshake.None,
                StopBits = StopBits.One,
                Encoding = Encoding.ASCII,
                DiscardNull = true,
                NewLine = "\r\n",
                ReadTimeout = 100000
            };

            _readingThread = new Thread(p => Read((CancellationToken)p)) { IsBackground = true };
            _processingThread = new Thread(p => Process((CancellationToken)p)) { IsBackground = true };
            _syncEvent = new AutoResetEvent(false);
            _cts = new CancellationTokenSource();
            _bufferStream = new MemoryStream();
            _reader = new StreamReader(_bufferStream, Encoding.ASCII);
        }

        public void Start()
        {
            _readingThread.Start(_cts.Token);
            _processingThread.Start(_cts.Token);
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private void Read(CancellationToken token)
        {
            _port.Open();
            _port.DataReceived += DataReceived;

            token.WaitHandle.WaitOne();
            _port.DataReceived -= DataReceived;

            void DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                byte[] data = new byte[_port.BytesToRead];
                _port.Read(data, 0, data.Length);
                lock (_bufferStream)
                {
                    _bufferStream.Seek(0, SeekOrigin.End);
                    _bufferStream.Write(data, 0, data.Length);
                    Console.WriteLine($"Buffer pos after direct write: {_bufferStream.Position}");
                }
                _syncEvent.Set();
                Console.WriteLine($"Serial data received ({data.Length} bytes)");
            }
        }

        private void Process(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                string line;
                try
                {
                    if (!_syncEvent.WaitOne(2500))
                    {
                        Console.WriteLine("Event wasn't set in the last 2.5 seconds.");
                        continue;
                    }

                    lock (_bufferStream)
                    {
                        _bufferStream.Position = _readingIndex;
                        line = _reader.ReadLine();
                        Console.WriteLine($"Buffer pos after indirect read: {_bufferStream.Position}");
                        if (line != null)
                        {
                            // If it's at the end right now, the line might not have
                            // been an actual line since the reader returns all the data if 
                            // it reaches the end of the stream without getting to a line break
                            // This has the consequence that the line will only be read after a
                            // new character (after the line break) is available.
                            if (_reader.EndOfStream)
                            {
                                _bufferStream.Position = _readingIndex;
                                line = null;
                            }
                            else
                            {
                                _readingIndex = _bufferStream.Position - 1;
                                Console.WriteLine($"Readingindex adjusted to {_readingIndex}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Couldn't complete read due to cancellation. Error:");
                    }
                    else
                    {
                        Console.WriteLine("Unexpected error:");
                    }
                    Console.WriteLine(e);

                    return;
                }

                if (line != null)
                {
                    Console.WriteLine($"Line read: {line}");
                }
                else
                {
                    Console.WriteLine("No line currently available.");
                }
            }
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();
                    _cts.Dispose();
                    _reader.Dispose();
                    _bufferStream.Dispose();
                    _port.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
