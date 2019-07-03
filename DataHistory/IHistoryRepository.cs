using System;
using DataHandler;
using System.Collections.Generic;
using System.Linq;

namespace DataHistory
{
    public interface IHistoryRepository
    {
        IQueryable<Data> GetDataBetween(DateTime from, DateTime to);
    }
}
