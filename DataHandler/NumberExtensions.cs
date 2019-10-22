using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler
{
    public static class NumberExtensions
    {
        public static float? ParseFloatOrNull(this string value) =>
            float.TryParse(value, out float result) ? result : (float?)null;

        public static int? ParseIntOrNull(this string value) =>
            int.TryParse(value, out int result) ? result : (int?)null;


        public static int RoundToNext(this int value, int interval) => 
            ((int)Math.Ceiling((double)value / interval)) * interval;

        public static int Clamp(this int value, int min, int max)
        {
            if (max <= min)
                throw new ArgumentException("The bottom value has to be lower than the top value.");

            return (value <= min) ? min : (value >= max) ? max : value;
        }
    }
}
