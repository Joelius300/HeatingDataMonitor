using System;
using System.Globalization;
using System.IO;

namespace MigrateExistingData
{
    class Program
    {
        // Helper application to convert csv exported from the old sqlite db
        // to csv which can be imported by postgresql
        static void Main(string[] args)
        {
            const string Format = "yyyy-MM-dd HH:mm:ss";
            string inputPath = args[0].Trim('"');
            string outputPath = Path.ChangeExtension(inputPath, ".converted.csv");

            using StreamReader reader = new StreamReader(inputPath);
            using StreamWriter writer = new StreamWriter(outputPath);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(";", 3);
                DateTime spsTime = DateTime.ParseExact(parts[1],
                                                       Format,
                                                       CultureInfo.InvariantCulture,
                                                       DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal);

                string updatedLine = $"{parts[0]};{parts[1]};{spsTime.ToString(Format)};{parts[2]}";
                writer.WriteLine(updatedLine);
            }
        }
    }
}
