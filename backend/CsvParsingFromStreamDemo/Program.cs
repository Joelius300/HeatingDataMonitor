using CsvHelper;
using CsvHelper.Configuration;
using HeatingDataMonitor.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsvParsingFromStreamDemo
{
    class Program
    {
        async static Task Main(string[] args)
        {
            //SerialPort port = new SerialPort()
            //{
            //    PortName = "COM2",
            //    BaudRate = 9600,
            //    DataBits = 8,
            //    Parity = Parity.None,
            //    Handshake = Handshake.None,
            //    StopBits = StopBits.One,
            //    Encoding = Encoding.ASCII,
            //    DiscardNull = true,
            //    NewLine = "\r\n",
            //    ReadTimeout = 100000
            //};

            //port.Open();
            //Console.WriteLine("start");
            //do
            //{
            //    string read;
            //    if (Console.ReadLine().Length == 0)
            //    {
            //        read = port.ReadExisting();
            //    }
            //    else
            //    {
            //        read = port.ReadLine();
            //    }

            //    Console.WriteLine(Regex.Escape(read));
            //} while (true);

            /* So here's a thing
             * I don't have two devices with serial port handy to test this so
             * I used a virtual port and putty. Putty only send CR in serial mode
             * and I wan't able to get it to send something else. So I adjusted the
             * NewLine to NewLine.CR expecting it to just work. However it always required
             * two CRs to actually go through. Then I finally found out that you can use
             * Ctrl+J to send LF and I could go back to CRLF and test with that.
             * Everything worked as expected. Out of curiosity, I also tried just LF
             * and that works perfectly fine as well. This seems to be a bug in CsvHelper.
             * 
             * TODO
             * Create a repro sample without all the fuzz and without serial port if possible
             * then open up a new issue for CsvHelper.
             */
            var options = new OptionsWrapper<SerialHeatingDataOptions>(
                new SerialHeatingDataOptions
                {
                    PortName = "COM2",
                    NewLine = NewLine.CRLF
                }
            );

            using SerialPortHeatingDataReceiver receiver = new SerialPortHeatingDataReceiver(options, new FakeLogger());
            receiver.DataReceived += (o, e) => Console.WriteLine($"NEW DATA: {e}");
            await receiver.StartAsync(default);

            Console.WriteLine("Enter to exit");
            Console.ReadLine();

            //var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            //{
            //    Delimiter = ";", // from options
            //    NewLine = NewLine.CRLF,
            //    IgnoreBlankLines = true,
            //    IgnoreQuotes = true,
            //    Encoding = Encoding.ASCII,
            //    HasHeaderRecord = false
            //};
            //csvConfig.RegisterClassMap<DataCsvMap>();

            //using CsvReader reader = new CsvReader(Console.In, csvConfig);

            //while (true)
            //{
            //    Console.WriteLine("reading");
            //    if (reader.Read())
            //    {
            //        var data = reader.GetRecord<Data>();
            //        Console.WriteLine(data);
            //    }
            //    else
            //    {
            //        Console.WriteLine("no data");
            //    }
            //}
        }
    }

    public class OptionsWrapper<T> : IOptions<T>
        where T : class, new()
    {
        public OptionsWrapper(T value) => Value = value;

        public T Value { get; }
    }

    public class FakeLogger : ILogger<SerialPortHeatingDataReceiver>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
            Console.WriteLine(exception);
            Console.WriteLine();
        }
    }
}
