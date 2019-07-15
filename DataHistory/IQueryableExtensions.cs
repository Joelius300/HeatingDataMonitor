using DataHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataHistory
{
    public static class IQueryableExtensions
    {
        public static IQueryable<Data> GetOnePerHour(this IQueryable<Data> value) =>
            value
            .Where(d =>
                !value
                .Any(d2 =>
                    d2.DatumZeit.Date == d.DatumZeit.Date &&
                    d2.DatumZeit.Hour == d.DatumZeit.Hour &&
                    d2.DatumZeit < d.DatumZeit));

        public static IQueryable<Data> GetOnePerDay(this IQueryable<Data> value) =>
            value
            .Where(d =>
                !value
                .Any(d2 =>
                    d2.DatumZeit.Date == d.DatumZeit.Date &&
                    d2.DatumZeit.Hour < d.DatumZeit.Hour));
    }
}
