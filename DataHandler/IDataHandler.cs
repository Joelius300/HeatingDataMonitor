using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    public interface IDataHandler : IDisposable
    {
        event Action Changed;
        Data CurrentData { get; }
        bool NoDataReceived { get; }
        int ExpectedReadInterval { get; }
    }
}
