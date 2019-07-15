using System;
using System.IO;

namespace RaspberryPIUtils
{
    public class RaspberryPI
    {
        private const string CURRENT_FREQ_FILE = "/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq";
        private const string MIN_FREQ_FILE = "/sys/devices/system/cpu/cpu0/cpufreq/scaling_min_freq";
        private const string MAX_FREQ_FILE = "/sys/devices/system/cpu/cpu0/cpufreq/scaling_max_freq";
        private const string CURRENT_TEMP_FILE = "/sys/class/thermal/thermal_zone0/temp";

        public int? GetCurrentFrequency() {
            int? i = GetIntFromFile(CURRENT_FREQ_FILE);
            if (i.HasValue)
            {
                return i.Value / 1000;
            }

            return null;
        }

        public int? GetMaxFrequency()
        {
            int? i = GetIntFromFile(MAX_FREQ_FILE);
            if (i.HasValue)
            {
                return i.Value / 1000;
            }

            return null;
        }

        public int? GetMinFrequency()
        {
            int? i = GetIntFromFile(MIN_FREQ_FILE);
            if (i.HasValue)
            {
                return i.Value / 1000;
            }

            return null;
        }

        public decimal? GetCurrentTemp() {
            int? i = GetIntFromFile(CURRENT_TEMP_FILE);
            if (i.HasValue)
            {
                return Decimal.Round((decimal)i.Value / 1000, 1);
            }

            return null;
        }

        private int? GetIntFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                if (int.TryParse(GetContentFromFile(filePath), out int content)) return content;
            }

            return null;
        }

        private string GetContentFromFile(string filePath)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(file))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
