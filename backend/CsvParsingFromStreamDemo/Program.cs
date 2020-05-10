using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
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
            SerialPort port = new SerialPort()
            {
                PortName = "COM2",
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

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                NewLine = NewLine.CRLF,
                IgnoreBlankLines = true,
                IgnoreQuotes = true,
                Encoding = Encoding.ASCII,
                HasHeaderRecord = false
            };
            csvConfig.RegisterClassMap<DataCsvMap>();

            //using (SerialPortCsvReader<Data> reader = new SerialPortCsvReader<Data>(new SerialPortWrapper(port), csvConfig, Encoding.ASCII, 64))
            ////using (SerialPortCsvReader<Asdf> reader = new SerialPortCsvReader<Asdf>(port, csvConfig, Encoding.ASCII, 16))
            //{
            //    reader.DataReceived += (o, e) => Console.WriteLine($"Received valid data: {e}");
            //    reader.Start();

            //    Console.WriteLine("Enter to stop");
            //    Console.ReadLine();

            //    reader.Stop();
            //}

            //var fakePort = new FakeSerialPort()
            //{
            //    Encoding = Encoding.ASCII,
            //    NewLine = "\r\n"
            //};
            using (SerialPortCsvReader<Data> reader = new SerialPortCsvReader<Data>(new SerialPortWrapper(port), csvConfig))
            {
                reader.DataReceived += (o, e) => Console.WriteLine($"Received valid data: {e}");
                reader.Start();

                //string line;
                //while ((line = Console.ReadLine()) != "stop")
                //{
                //    line = line.Replace("\\r", "\r").Replace("\\n", "\n");
                //    fakePort.AddData(line);
                //}

                Console.WriteLine("Enter to stop");
                Console.ReadLine();

                reader.Stop();
            }

            //using (ParseFromMemoryStream pfms = new ParseFromMemoryStream())
            //{
            //    pfms.Start(); // blocks until stop is written which will then Stop

            //    pfms.Stop(); // redundant
            //} // redundant again

            //using (ReadFromPortIdea1 rfp = new ReadFromPortIdea1("COM2"))
            //{
            //    rfp.Start();

            //    Console.WriteLine("Enter to stop");
            //    Console.ReadLine();

            //    rfp.Stop();
            //}

            Console.WriteLine("Enter to exit");
            Console.ReadLine();

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
             * After investigating CsvHelper a bit, I think the error is in CsvParser.cs Line 297/298.
             * The Method ReadLineEnding returns the offset that needs to be applied but the return value
             * isn't used at all. The next call fieldReader.SetFieldStart() takes an int as offset which
             * defaults to 0. Seeing that ReadLineEnding will always return 0 EXCEPT for when the char is \r
             * and the following isn't \n (exactly what my case), then it would return -1. That -1 however is
             * just thrown away instead of passed to fieldReader.SetFieldStart().
             * 
             * TODO
             * Create a repro sample without all the fuzz and without serial port if possible
             * then open up a new issue for CsvHelper.
             */
            //var options = new OptionsWrapper<SerialHeatingDataOptions>(
            //    new SerialHeatingDataOptions
            //    {
            //        PortName = "COM2",
            //        NewLine = NewLine.CRLF
            //    }
            //);

            //using SerialPortHeatingDataReceiver receiver = new SerialPortHeatingDataReceiver(options, new FakeLogger());
            //receiver.DataReceived += (o, e) => Console.WriteLine($"NEW DATA: {e}");
            //await receiver.StartAsync(default);

            //Console.WriteLine("Enter to exit");
            //Console.ReadLine();

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

    public class Asdf
    {
        [Index(0)]
        public string A { get; set; }
        [Index(1)]
        public string B { get; set; }
        [Index(2)]
        public string C { get; set; }
        [Index(3)]
        public string D { get; set; }
    }

    public class OptionsWrapper<T> : IOptions<T>
        where T : class, new()
    {
        public OptionsWrapper(T value) => Value = value;

        public T Value { get; }
    }

    //public class FakeLogger : ILogger<SerialPortHeatingDataReceiver>
    //{
    //    public IDisposable BeginScope<TState>(TState state)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool IsEnabled(LogLevel logLevel)
    //    {
    //        return true;
    //    }

    //    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    //    {
    //        Console.WriteLine(formatter(state, exception));
    //        Console.WriteLine(exception);
    //        Console.WriteLine();
    //    }
    //}
}
