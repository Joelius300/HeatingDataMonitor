using System;
using System.Collections.Generic;
using System.Linq;
using DataHandler;

namespace DataHistory
{
    public class HistoryRepository : IHistoryRepository
    {
        protected HeatingDataContext Context { get; }

        public HistoryRepository(HeatingDataContext context) => Context = context;

        public IEnumerable<Data> GetDataBetween(DateTime from, DateTime to) => 
            Context.Data.Where(d => (d.DatumZeit >= from && d.DatumZeit <= to));
    }
}
