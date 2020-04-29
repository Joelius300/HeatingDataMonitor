using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CsvParsingFromStreamDemo
{
    class DataCsvMap : ClassMap<Data>
    {
        // 10.12.08;00:01:50;10.4;3.1;19;OnePointFive;
        // 10.12.08;00:01:50;10.4;3.1;19;2;
        public DataCsvMap()
        {
            Map(d => d.Time).ConvertUsing(ParseHeatingDataTime);
            Map(d => d.Temp).Index(2);
            Map(d => d.Pressure).Index(3);
            Map(d => d.SomeInt).Index(4);
            Map(d => d.SomeEnum).Index(5);
        }

        private static DateTime ParseHeatingDataTime(IReaderRow row)
        {
            const string DateTimeFormat = "dd.MM.yy HH:mm:ss";

            string datePart = row.GetField<string>(0);
            string timePart = row.GetField<string>(1);

            return DateTime.ParseExact($"{datePart} {timePart}", DateTimeFormat, CultureInfo.InvariantCulture);
        }
    }
}
