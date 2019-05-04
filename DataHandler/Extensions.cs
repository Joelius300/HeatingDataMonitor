using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    public static class Extensions
    {
        public static string GetString<T>(this T? value) where T : struct
        {
            if (value.HasValue) return value.ToString();
            return "#####";
        }

        public static string GetString<T>(this T? value, string unit) where T : struct
        {
            if (value.HasValue) return $"{value.ToString()} {unit}";
            return "#####";
        }

        public static string GetStringShort<T>(this T? value, string unit) where T : struct
        {
            if (value.HasValue) return $"{value.ToString()} {unit}";
            return "###";
        }
    }
}
