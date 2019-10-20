using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    class FaultyDataReceivedException : Exception
    {
        public string FaultyData { get; }

        public FaultyDataReceivedException(string faultyData) : base("Es wurden falsche Daten empfangen.")
        {
            FaultyData = faultyData;
        }
    }
}
