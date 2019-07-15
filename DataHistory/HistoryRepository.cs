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


        public void Add(Data data)
        {
            Context.Data.Add(data);
            Context.SaveChanges();
        }


        public IQueryable<Data> GetDataBetween(DateTime from, DateTime to) => 
            Context.Data.Where(d => (d.DatumZeit >= from && d.DatumZeit <= to));
    }
}
