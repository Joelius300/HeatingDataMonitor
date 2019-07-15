using System;
using DataHandler;
using System.Collections.Generic;
using System.Linq;

namespace DataHistory
{
    public interface IHistoryRepository
    {
        void Add(Data data);
        IQueryable<Data> GetDataBetween(DateTime from, DateTime to);
    }
}
