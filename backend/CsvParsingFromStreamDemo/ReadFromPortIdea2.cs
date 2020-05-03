using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace CsvParsingFromStreamDemo
{
    public class ReadFromPortIdea2 : IDisposable
    {
        private const string NewLine = "\r\n";
        private readonly SerialPort _port;
        private readonly Thread _readingThread;
        private readonly Thread _processingThread;
        private readonly AutoResetEvent _syncEvent;
        private readonly CancellationTokenSource _cts;
        private readonly MemoryStream _bufferStream;
        private readonly StreamReader _reader;
        private long _readingIndex;

        public ReadFromPortIdea2(string name)
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
                NewLine = NewLine,
                ReadTimeout = 100000
            };

            _readingThread = new Thread(p => Read((CancellationToken)p)) { IsBackground = true };
            _processingThread = new Thread(p => Process((CancellationToken)p)) { IsBackground = true };
            _syncEvent = new AutoResetEvent(false);
            _cts = new CancellationTokenSource();
            _bufferStream = new MemoryStream();
            _reader = new StreamReader(_bufferStream, Encoding.ASCII);
            // CsvReader reader = new CsvReader(_reader, null);
            // reader.Read()
        }

        public void Start()
        {
            _port.Open();
            _readingThread.Start(_cts.Token);
            _processingThread.Start(_cts.Token);

            /* The general idea is as follows:
             * I hope my assumptions on this stuff is correct.
             * There are two threads (one for reading from the port, one from parsing the records)
             * There is one buffer which is used for all the bytes read from the serial port. The
             * CsvReader reads from that buffer, not directly from the serial port.
             * Reading thread:
             *  All the data from the serial port is constantly added to the buffer (probably easiest
             *  with the DataReceived-event). If the buffer gets above a certain size (I think above
             *  one mb or so), it is cleared but that doesn't necessarily have to be signaled here.
             *  
             * Parsing thread:
             *  Calls Read on the CsvReader which will try to advance to the line break. If there is no
             *  line break available, it will return false. It still advances the reader so the position in
             *  the stream has to be reset to the original one before reading again. Since this position is
             *  bound to the stream, it will also affect writing. Therefore it's very important to synchronize
             *  reading and writing at all times because the position has to be adjusted for both.
             *  If advancing fails (the underlying stream is closed for example) we won't be able to recover
             *  and need to abort. This shouldn't be an issue since this will only be the case when we cancel
             *  reading and dispose the buffer. In that case it's expected that Read will throw _something_
             *  since its's underlying buffer is modified which causes undefined behaviour and usually some
             *  sort of disposed or null reference exception. When that happens, we just back out and let the
             *  thread finish as gracefully as possible. This is probably not as bad as if it happened directly
             *  on the serial port since a memory stream doesn't actually have to be disposed, the underlying
             *  buffer just has to be GC'd which will happen either way.
             *  Once the reader advances, the line is parsed. If the parsing fails, just log and go back
             *  to reading since it's very possible that you receive wrong data (the port constantly pumps
             *  out data and we might join in the middle).
             *  After parsing we check if we need to reset the buffer.
             *  
             * Clearing of the buffer:
             *  When clearing the buffer to lower memory consumption, there are a few key things to
             *  keep in mind.
             *   - While you're clearing the buffer, you can't be in a CsvHelper.Read() call. We should
             *     probably check the buffers size whenever such a Read-call finishes and to the clearing
             *     on the parsing thread.
             *   - While you're clearing the buffer, we can't be adding new data so that has to paused
             *     by some sort of ThreadSyncEvent.
             *   - Clearing the memory stream should be done with SetLength and setting Position since
             *     that will retain the capacity and reuse the underlying array without additional
             *     allocations. https://stackoverflow.com/questions/2462391/reset-or-clear-net-memorystream
             *   - Any data that hasn't been parsed yet has to be kept in the buffer. One way to do so could be to
             *     store the position in the buffer the last record ended and keep all the data after that.
             *     The position property will most likely be influenced by the CsvReader so we should be able
             *     to just read it out after Read() completes.
             */
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        //private void Read(CancellationToken token)
        //{
        //    while (!token.IsCancellationRequested)
        //    {
        //        _syncEvent.WaitOne(); // wait for the parsing to be done

        //        byte[] data = new byte[_port.BytesToRead];
        //        _port.Read(data, 0, data.Length);
        //        _bufferStream.Seek(0, SeekOrigin.End);
        //        _bufferStream.Write(data, 0, data.Length);

        //        Console.WriteLine($"Buffer pos after direct write: {_bufferStream.Position}");
        //        Console.WriteLine($"Serial data received ({data.Length} bytes)");

        //        _syncEvent.Set();
        //    }
        //}

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
                _bufferStream.Seek(0, SeekOrigin.End);
                _bufferStream.Write(data, 0, data.Length);
                Console.WriteLine($"Buffer pos after direct write: {_bufferStream.Position}");
                Console.WriteLine($"Serial data received ({data.Length} bytes)");
                _syncEvent.Set();
            }
        }

        private void Process(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _syncEvent.WaitOne(); // wait for the reading to be done


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
