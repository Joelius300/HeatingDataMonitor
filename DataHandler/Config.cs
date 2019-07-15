using DataHandler.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace DataHandler
{
    public class Config
    {
        public string HostIP { get; set; }
        public int Port { get; set; }
        public string SerialPortName { get; set; }
        public int ExpectedReadInterval { get; set; }

        [DefaultValue(null)]
        [XmlElement(IsNullable = true)]
        public int? HistorySaveDelayInMinutes { get; set; } = null;

        [DefaultValue(null)]
        [XmlElement(IsNullable = true)]
        public string HistorySQLiteConnectionString { get; set; } = null;


        private const string PATH = "DataConfig.xml";
        public void Serialize(string path = PATH) {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(file, this);
            }
        }

        public static Config Deserialize(string path = PATH) {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            try
            {
                return (Config)serializer.Deserialize(file);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (InvalidCastException) {
                throw;
            }
            finally
            {
                file.Dispose();
            }
        }

        public const int DEFAULT_EXPECTED_READ_INTERVAL = 6;

        public static Config GetDefaultWithHistory() {
            return new Config() {
                SerialPortName = "COM1",
                ExpectedReadInterval = DEFAULT_EXPECTED_READ_INTERVAL,
                HostIP = "xxx.xxx.xxx.xxx",
                Port = 5000,
                HistorySaveDelayInMinutes = 10,
                HistorySQLiteConnectionString = $"Data Source={Path.Combine(Environment.CurrentDirectory, "History.db")};"
            };
        }

        public static Config GetDefaultWithoutHistory()
        {
            return new Config()
            {
                SerialPortName = "COM1",
                ExpectedReadInterval = DEFAULT_EXPECTED_READ_INTERVAL,
                HostIP = "xxx.xxx.xxx.xxx",
                Port = 5000
            };
        }

        public void CheckConfig()
        {
            IEnumerable<string> ports = SerialPort.GetPortNames();
            if (!ports.Contains(this.SerialPortName)) throw new NonExistingSerialPortException(this.SerialPortName);

            if (!Regex.Match(HostIP, @"^(\d{1,3}\.){3}\d{1,3}$").Success || !IPAddress.TryParse(HostIP, out _)) throw new InvalidIPAddressException(HostIP);

            if (this.ExpectedReadInterval < 1 || this.ExpectedReadInterval > 60)
            {
                int old = this.ExpectedReadInterval;
                this.ExpectedReadInterval = Config.DEFAULT_EXPECTED_READ_INTERVAL;
                Console.WriteLine($"Das angegebene Leseintervall ({old}s) ist nicht valid. Es wurde auf {ExpectedReadInterval}s korrigiert.");
            }
        }
    }
}
