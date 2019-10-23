using System.Net;
using CloudAtlas.Model.Exceptions;
using MessagePack;

namespace CloudAtlas.Model
{
    [MessagePackObject]
    public class ValueContact : Value
    {
        [Key(11)]
        public PathName Name { get; }
        [Key(12)]
        public IPAddress Address { get; } // TODO: Check equivalence with InetAddress

        private ValueContact() {}
        public ValueContact(PathName name, IPAddress address)
        {
            Name = name;
            Address = address;
        }
        
        [IgnoreMember] public override AttributeType AttributeType => AttributeTypePrimitive.Contact;
        [IgnoreMember] public override bool IsNull => Name == null || Address == null;
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