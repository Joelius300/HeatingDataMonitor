using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DataHandler
{
    public class Config
    {
        public string SerialPortName { get; set; }

        public int ExpectedReadTimeout { get; set; }

        private const string PATH = "DataConfig.xml";
        public void Serialize() {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using FileStream file = new FileStream(PATH, FileMode.Create, FileAccess.Write);
            Console.WriteLine(file.Name);
            serializer.Serialize(file, this);
        }

        public static Config Deserialize() {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            try
            {
                using FileStream file = new FileStream(PATH, FileMode.Open, FileAccess.Read);
                Console.WriteLine(file.Name);
                return (Config)serializer.Deserialize(file);
            }
            catch (FileNotFoundException) {
                Config conf = GetDefault();
                conf.Serialize();
                return conf;
            }
        }

        public static Config GetDefault() {
            return new Config() { ExpectedReadTimeout = 6000, SerialPortName = "COM1" };
        }
    }
}
