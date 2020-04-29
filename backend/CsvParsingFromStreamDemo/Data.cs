using System;
using System.Collections.Generic;
using System.Text;

namespace CsvParsingFromStreamDemo
{
    class Data
    {
        public float? Temp { get; set; }
        public float? Pressure { get; set; }
        public int SomeInt { get; set; }
        public SomeEnum SomeEnum { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"Temp: {Temp}, Pressure: {Pressure}, SomeInt: {SomeInt}, SomeEnum: {SomeEnum}/{(int)SomeEnum}, Time: {Time}";
        }
    }
}
