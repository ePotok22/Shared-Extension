using System;
using System.ComponentModel;

namespace FFF.Shared
{
    [AttributeUsage(AttributeTargets.All)]
    public class ValueInfomationAttribute : DescriptionAttribute
    {
        public ValueInfomationAttribute(object value, string description)
        {
            DescriptionValue = description;
            Value = value;
        }

        public ValueInfomationAttribute(string description)
        {
            DescriptionValue = description;
        }

        public ValueInfomationAttribute(object value)
        {
            Value = value;
        }

        public override string Description =>
            DescriptionValue;

        public object Value { get; private set; }
    }
}
