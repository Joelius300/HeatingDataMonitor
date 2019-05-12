using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler.Exceptions
{
    class NoDataReceivedException : Exception
    {
        private const string MSG = "Es wurden keine Daten empfangen.";

        public NoDataReceivedException() : base(MSG)
        {
        }

        public NoDataReceivedException(Exception innerException) : base(MSG, innerException)
        {
        }
    }
}