using System;
using DataHandler;
using System.Collections.Generic;

namespace DataHistory
{
    interface IHistoryRepository
    {
        IEnumerable<Data> GetDataBetween(DateTime from, DateTime to);
    }
}
