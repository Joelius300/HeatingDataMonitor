using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler.Exceptions
{
    class FaultyDataReceivedException : Exception
    {
        public string FaultyData { get; private set; }

        public FaultyDataReceivedException(string faultyData) : base("Es wurden falsche Daten empfangen.")
        {
            FaultyData = faultyData;
        }
    }
}
