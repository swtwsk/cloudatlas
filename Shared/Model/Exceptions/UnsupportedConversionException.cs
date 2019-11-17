using System;

namespace Shared.Model.Exceptions
{
    public class UnsupportedConversionException : NotSupportedException
    {
        public AttributeType From { get; }
        public AttributeType To { get; }

        public UnsupportedConversionException(AttributeType from, AttributeType to) 
            : base($"Type {from} cannot be converted to {to}.")
        {
            From = from;
            To = to;
        }
    }
}