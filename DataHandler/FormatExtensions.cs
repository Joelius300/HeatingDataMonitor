using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    public static class FormatExtensions
    {
        public const string NullPlaceholder = "#####";

        public static string OrPlaceholder<T>(this T value, string nullPlaceholder = NullPlaceholder)
        {
            if (value == null) return nullPlaceholder;
            return value.ToString();
        }

        public static string WithUnit<T>(this T value, string unit, string nullPlaceholder = NullPlaceholder)
        {
            if (value == null) return nullPlaceholder;
            return $"{value.ToString()} {unit}";
        }
    }
}
