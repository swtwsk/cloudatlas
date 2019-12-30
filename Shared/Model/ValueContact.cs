using System;
using System.Net;
using Shared.Model.Exceptions;

namespace Shared.Model
{
    public class ValueContact : Value
    {
        public PathName Name { get; private set; }
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }

        private ValueContact() {}
        public ValueContact(PathName name, IPAddress address, int port = 5555)
        {
            Name = name;
            Address = address;
            Port = port;
        }
        
        public override AttributeType AttributeType => AttributeTypePrimitive.Contact;
        public override bool IsNull => Name == null || Address == null;
        public override Value ConvertTo(AttributeType to)
        {
            return to.PrimaryType switch
            {
                PrimaryType.Contact => this as Value,
                PrimaryType.String => IsNull
                    ? ValueString.NullString
                    : new ValueString($"({Name}, {Address}:{Port})"),
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ValueContact valueContact))
                return false;

            return Name.Equals(valueContact.Name) && Address.Equals(valueContact.Address) && Port == valueContact.Port;
        }

        public override Value GetDefaultValue() => new ValueContact(null, null);
    }
}