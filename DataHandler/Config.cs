using DataHandler.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace DataHandler
{
    public class Config
    {
        public string HostIP { get; set; }
        public int Port { get; set; }
        public string SerialPortName { get; set; }
        public int ExpectedReadInterval { get; set; }

        private const string PATH = "DataConfig.xml";
        public void Serialize() {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using FileStream file = new FileStream(PATH, FileMode.Create, FileAccess.Write);
            serializer.Serialize(file, this);
        }

        public static Config Deserialize() {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            try
            {
                using FileStream file = new FileStream(PATH, FileMode.Open, FileAccess.Read);
                return (Config)serializer.Deserialize(file);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (InvalidCastException) {
                throw;
            }
        }

        public const int DEFAULT_EXPECTED_READ_INTERVAL = 6000;

        public static Config GetDefault() {
            return new Config() {
                SerialPortName = "COM1",
                ExpectedReadInterval = DEFAULT_EXPECTED_READ_INTERVAL,
                HostIP = "192.168.38.121",
                Port = 5000,
            };
        }

        public void CheckConfig()
        {
            IEnumerable<string> ports = SerialPort.GetPortNames();
            if (!ports.Contains(this.SerialPortName)) throw new NonExistingSerialPortException(this.SerialPortName);

            if (!IPAddress.TryParse(HostIP, out _) && HostIP != "localhost") throw new InvalidIPAddressException(HostIP);

            if (this.ExpectedReadInterval < 1000 || this.ExpectedReadInterval > 600000)
            {
                int old = this.ExpectedReadInterval;
                this.ExpectedReadInterval = Config.DEFAULT_EXPECTED_READ_INTERVAL;
                Console.WriteLine($"Das angegebene Leseintervall ({old}ms) ist nicht valid. Es wurde auf {ExpectedReadInterval}ms korrigiert.");
            }
        }
    }
}
