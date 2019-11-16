using System.Net;
using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class ValueContact : Value
    {
        public PathName Name { get; private set; }
        public IPAddress Address { get; private set; } // TODO: Check equivalence with InetAddress

        private ValueContact() {}
        public ValueContact(PathName name, IPAddress address)
        {
            Name = name;
            Address = address;
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
                    : new ValueString($"({Name.ToString()}, {Address.ToString()})"),
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }

        public override Value GetDefaultValue() => new ValueContact(null, null);
    }
}