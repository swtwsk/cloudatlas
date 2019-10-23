using System;
using System.Collections.Generic;

namespace CloudAtlas.Model
{
    public class AttributeTypeCollection : AttributeType
    {
        public AttributeType ElementType { get; private set; }
     
        private AttributeTypeCollection() {}
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

        public static AttributeType ComputeElementType(IList<Value> collection)
        {
            AttributeType mainType = null;

            for (var i = 0; i < collection.Count; i++)
            {
                if (collection[i].IsNull)
                    collection[i] = ValueNull.Instance;
                if (mainType == null)
                {
                    if (collection[i].AttributeType.PrimaryType != PrimaryType.Null)
                        mainType = collection[i].AttributeType;
                }
                else if (!mainType.IsCompatible(collection[i].AttributeType))
                    throw new ArgumentException("Collection has non-null elements of different types.");
            }

            return mainType ?? AttributeTypePrimitive.Null;
        }
    }
}