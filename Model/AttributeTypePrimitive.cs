using System;
using MessagePack;

namespace CloudAtlas.Model
{
    [MessagePackObject()]
    public class AttributeTypePrimitive : AttributeType
    {
        public static AttributeTypePrimitive Boolean = new AttributeTypePrimitive(PrimaryType.Boolean);
        public static AttributeTypePrimitive Contact = new AttributeTypePrimitive(PrimaryType.Contact);
        public static AttributeTypePrimitive Double = new AttributeTypePrimitive(PrimaryType.Double);
        public static AttributeTypePrimitive Duration = new AttributeTypePrimitive(PrimaryType.Duration);
        public static AttributeTypePrimitive Integer = new AttributeTypePrimitive(PrimaryType.Int);
        public static AttributeTypePrimitive Null = new AttributeTypePrimitive(PrimaryType.Null);
        public static AttributeTypePrimitive String = new AttributeTypePrimitive(PrimaryType.String);
        public static AttributeTypePrimitive Time = new AttributeTypePrimitive(PrimaryType.Time);

        private AttributeTypePrimitive(PrimaryType primaryType) : base(primaryType)
        {
            switch (primaryType)
            {
                case PrimaryType.Boolean:
                case PrimaryType.Contact:
                case PrimaryType.Double:
                case PrimaryType.Duration:
                case PrimaryType.Int:
                case PrimaryType.Null:
                case PrimaryType.String:
                case PrimaryType.Time:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primaryType), primaryType,
                        "This class can represent a primitive type only (boolean, int etc.).");
            }
        }

        public override string ToString() => PrimaryType.ToString();

        public override bool IsCompatible(AttributeType type) =>
            base.IsCompatible(type) || PrimaryType == type.PrimaryType;
    }
}