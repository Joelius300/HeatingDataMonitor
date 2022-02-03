using System.IO.Ports;

namespace HeatingDataMonitor.Receiver;

public class SerialPortOptions
{
    // Defaults from:
    // https://github.com/dotnet/runtime/blob/f185bd5571f2500b4843d145418c4418747246b6/src/libraries/System.IO.Ports/src/System/IO/Ports/SerialPort.cs
    private const int DefaultDataBits = 8;
    private const Parity DefaultParity = Parity.None;
    private const StopBits DefaultStopBits = StopBits.One;
    private const Handshake DefaultHandshake = Handshake.None;
    private const int DefaultBaudRate = 9600;
    private const string DefaultNewLine = "\n";
    private const string DefaultEncoding = "us-ascii";
    private const string DefaultDelimiter = ";";

    public string PortName { get; set; } = null!;
    public string Delimiter { get; set; } = DefaultDelimiter;
    public int BaudRate { get; set; } = DefaultBaudRate;
    public int DataBits { get; set; } = DefaultDataBits;
    public Parity Parity { get; set; } = DefaultParity;
    public StopBits StopBits { get; set; } = DefaultStopBits;
    public Handshake Handshake { get; set; } = DefaultHandshake;
    public string NewLine { get; set; } = DefaultNewLine;
    public string Encoding { get; set; } = DefaultEncoding;
}
