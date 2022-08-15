# Improvements and ideas

There are many improvements that could be made to the implementation of parsing the heating data.
The fact that they are listed here and not implemented is due to the already large amount of time spent with this implementation including tests, research, hardware-fiddling, etc. and the fact that most of these improvements bring little to no benefit _for this project_.

## Tighter abstraction around serial port

Instead of having an `ICsvHeatingDataReader` interface, one could have an `IChunkedSerialPortReader` or something like that which will return chunks of data read by the serial port.

Benefits:

-   Easier to test because even more logic can be tested without actual serial port
-   Easier to implement safeguard to only return full lines (boils down to testing again)
-   Tighter abstractions are cleaner

Disadvantages:

-   One of many solutions that are "a bit cleaner" but still don't feel like "the right way to do this".

## Moving from strings to bytes

The `CsvReader` wants a `TextReader` anyway which will handle byte to text conversion. This makes the abstractions even tighter and it will increase performance and decrease memory usage; both things that aren't really relevant in this project because of the slow rate and low amount of data transmission. You could argue that we're running this off of a raspberry pi so performance is more important than in other scenarios but I don't think there would be an observable difference.

Benefits:

-   Performance and memory usage

Disadvantages:

-   Slightly harder to test and debug

## Throw it all out and attempt parsing csv directly from the serial port stream again

The last time I tried this it started a journey down a rabbit hole which lead to countless hours investigating the inner workings of serial ports (in .NET, Windows and Linux) and the CsvHelper library. While I did learn a lot from that, I don't want to do that again over 2 years later.
Maybe things have changed but considering this is my third rework and I've already sunken most of my time into the serial port reading, I don't want to open that can of worms again.

In theory it should be simple. Open the serial port, create a stream reader over the `BaseStream` property of the serial port and just parse until I say stop (there's `GetRecordsAsync<T>(CancellationToken): IAsyncEnumerable<T>`). And if it works it's absolutely the simplest and cleanest solution there is. But if anything doesn't quite work as expected, you have no layers to fall back to and investigate what could be wrong because there are 0 abstractions which makes debugging a lot harder. There's also not a lot you could test, you'll need to rely on the simplicity of the implementation and the correctness of the serial port and CsvParser.

I still believe it is possible (and maybe even "the right way"), why should't it be, but I don't know if I'll ever get back to this because I don't need the additional performance, the current solutions works and I've put a lot of effort into documenting the current implementation to minimize issues later on.

## Generics

There are multiple places where generics could be used because things are not tied to a certain class (this also extends outside of the receiver). However, this just doesn't add any benefit for this project as `HeatingData` will continue to be the only db model we have to worry about and this model can even be updated if columns were to change.

# Setup for testing serial port receiver

I did this today 08.02.22 and everything seems to work. I will continue with the parser and then db-insertion, which can luckily both be tested completely without a serial port.

-   Install Raspberry PI OS Lite 64-bit
-   `sudo apt update && sudo apt upgrade`
-   `sudo raspi-config`
    -   Interfacing Options -> Serial
        -   Login Shell: No
        -   Enable hardware: Yes
-   Reboot
-   Edit `/boot/config.txt` and add `dtoverlay=disable-bt
-   Configure minicom with `sudo minicom -s` (don't forget to save)
    -   Device: `/dev/ttyAMA0`
    -   Speed: 9600
    -   Parity: None
    -   Data: 8
    -   Stopbits: 1
    -   (you need to configure minicom the same way on the host pc obviously, also for my Manjaro PC I needed "Hardware Flow Control" disabled)
-   Now you can open minicom on both raspi and host and see if you receive the data correctly.
-   Next, use the receiver to check if everything works
    -   Add tons of logging statements to the SerialPortCsvHeatingDataReceiver
    -   Use a simple test-worker, which uses an await foreach and just logs each line it gets
    -   Install .NET on raspi (https://docs.microsoft.com/en-us/dotnet/iot/deployment)
    -   Copy necessary source files over and adjust some csproj stuff
    -   Use `dotnet run` to run the reading loop
    -   Use minicom on the host to send data. IMPORTANT: Dont use Enter, it sends a CR (\r), not a LF (\n). To send a newline, you need to use Ctrl+j (at least on my system).
    -   After that works correctly, use `echo -n "blabla\nblabla" > /dev/ttyS0` and similar to ensure receiving junks of data, potentially including line-breaks works correctly
