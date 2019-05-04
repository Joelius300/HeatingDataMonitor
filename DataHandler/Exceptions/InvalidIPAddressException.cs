using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler.Exceptions
{
    public class InvalidIPAddressException : Exception
    {
        public string IP { get; set; }

        public InvalidIPAddressException(string ip) : base($"\"{ip}\" ist keine gültige IP-Adresse.")
        {
            IP = ip;
        }
    }
}
