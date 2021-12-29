﻿using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

// https://stackoverflow.com/questions/52187/virtual-serial-port-for-linux

Console.Write("File: ");
string file = Console.ReadLine()!;
Console.Write("Port: ");
string portName = Console.ReadLine()!;
Console.Write("Baud: ");
int baud = int.Parse(Console.ReadLine()!);
Console.Write("Handshake: ");
Handshake handshake = Enum.Parse<Handshake>(Console.ReadLine()!);
Console.Write("Parity: ");
Parity parity = Enum.Parse<Parity>(Console.ReadLine()!);
Console.Write("StopBits: ");
StopBits stopBits = Enum.Parse<StopBits>(Console.ReadLine()!);
Console.Write("Interval: ");
int interval = int.Parse(Console.ReadLine()!);

using SerialPort port = new()
{
    PortName = portName,
    BaudRate = baud,
    Handshake = handshake,
    Parity = parity,
    StopBits = stopBits
};

await using FileStream fileStream = new(file, FileMode.Open);

using CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

port.Open();

Console.WriteLine("Ctrl + C to stop");

Random rng = new();
CancellationToken token = cts.Token;
byte[] buffer = new byte[32];

while (!token.IsCancellationRequested)
{
    int length = rng.Next(buffer.Length / 2, buffer.Length + 1);
    fileStream.Read(buffer, 0, length);
    port.Write(buffer, 0, length);
    Console.WriteLine($"Sent {length} bytes to {portName}.");

    try
    {
        await Task.Delay(interval, token);
    }
    catch (TaskCanceledException)
    {
        break;
    }
}

port.Close();
