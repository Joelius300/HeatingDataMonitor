namespace HeatingDataMonitor.Receiver;

internal class FileCsvHeatingDataReader : ICsvHeatingDataReader, IAsyncEnumerable<string>
{
    private readonly string _path;
    private readonly int _delay;

    public FileCsvHeatingDataReader(string path, int delay)
    {
        _path = path;
        _delay = delay;
    }

    // We cannot use [EnumeratorCancellation] because that requires a CancellationToken in
    // the signature. Instead you implement IAsyncEnumerable and use the token from GetAsyncEnumerator.
    public IAsyncEnumerable<string> ReadCsvLines() => this;

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        const int defaultBufferSize = 4096;
        await using FileStream fileStream = new(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
            defaultBufferSize, FileOptions.Asynchronous);
        using StreamReader reader = new(fileStream);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (reader.EndOfStream)
            {
                // we only want to stop sending lines when the token is cancelled, so go back to the beginning when we're through
                fileStream.Seek(0, SeekOrigin.Begin);
            }

            string? line = await reader.ReadLineAsync();
            // in files we allow empty lines (by ignoring them) but the serial port will never send empty lines
            if (string.IsNullOrEmpty(line))
                continue;

            yield return line;
            try
            {
                await Task.Delay(_delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                yield break;
            }
        }
    }
}
