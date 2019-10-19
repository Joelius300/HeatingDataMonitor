using System;
using System.Collections.Generic;
using System.Text;

namespace DataHandler.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class DisplayableValueAttribute : Attribute
    {
        public string DisplayText { get; }

        public string? Unit { get; set; } = null;

        public bool IsRelevant { get; set; } = true;

        public DisplayableValueAttribute(string displayText)
        {
            DisplayText = displayText;
        }
    }
}
