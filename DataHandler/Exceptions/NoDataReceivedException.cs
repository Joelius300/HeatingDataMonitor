using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler.Exceptions
{
    class NoDataReceivedException : Exception
    {
        private const string msg = "Es wurden keine Daten empfangen.";

        public NoDataReceivedException() : base(msg)
        {
        }

        public NoDataReceivedException(Exception innerException) : base(msg, innerException)
        {
        }
    }
}