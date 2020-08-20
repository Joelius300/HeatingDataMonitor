namespace HeatingDataMonitor.Backup.Services
{
    public interface ISevenZipFactory
    {
        ISevenZip Create(string executablePath);
    }
}