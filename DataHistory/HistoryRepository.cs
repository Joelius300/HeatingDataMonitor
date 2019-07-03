using System;
using System.Collections.Generic;
using System.Linq;
using DataHandler;

namespace DataHistory
{
    public class HistoryRepository : IHistoryRepository
    {
        public IQueryable<Data> GetDataBetween(DateTime from, DateTime to)
        {
            if (HeatingDataContext.Instance == null) throw new InvalidOperationException("The Database-context has not yet been initialized.");
            return HeatingDataContext.Instance.Data.Where(d => (d.DatumZeit.HasValue && d.DatumZeit.Value >= from && d.DatumZeit.Value <= to));
        }
    }
}
