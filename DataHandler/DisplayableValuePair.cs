using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    public struct DisplayableValuePair
    {
        public string Header { get; }
        public string DisplayValue { get; }

        public DisplayableValuePair(string header, string displayValue)
        {
            Header = header;
            DisplayValue = displayValue;
        }
    }
}
