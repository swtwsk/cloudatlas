using System;
using System.Collections.Generic;

namespace CloudAtlas.Model
{
    public class AttributeTypeCollection : AttributeType
    {
        public AttributeType ElementType { get; }
        
        public AttributeTypeCollection(PrimaryType primaryType, AttributeType elementType) : base(primaryType)
        {
            switch (primaryType)
            {
                case PrimaryType.List:
                case PrimaryType.Set:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primaryType), primaryType,
                        "This class can represent a collection only (list, set etc.).");
            }

            ElementType = elementType;
        }

        public override string ToString() => $"{PrimaryType.ToString()} of ({ElementType.ToString()})";

        public override bool IsCompatible(AttributeType type) =>
            base.IsCompatible(type) || PrimaryType == type.PrimaryType &&
            ElementType.IsCompatible(((AttributeTypeCollection) type).ElementType);

        public override bool IsCollection() => true;

        public static AttributeType ComputeElementType(IEnumerable<Value> collection)
        {
            AttributeType mainType = null;

            foreach (var v in collection)
            {
                if (v.IsNull)
                    v = ValueNull.Instance;
                if (mainType == null)
                {
                    if (v.AttributeType.PrimaryType != PrimaryType.Null)
                        mainType = v.AttributeType;
                }
                else if (!mainType.IsCompatible(v.AttributeType))
                    throw new ArgumentException("Collection has non-null elements of different types.");
            }

            return mainType ?? AttributeTypePrimitive.Null;
        }
    }
}