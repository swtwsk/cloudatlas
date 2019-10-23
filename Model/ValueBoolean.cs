using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class ValueBoolean : ValueSimple<RefStruct<bool>>
    {
        private ValueBoolean() {}
        public ValueBoolean(RefStruct<bool> value) : base(value) {}
        public ValueBoolean(bool value) : base(value) {}
        
        public override AttributeType AttributeType => AttributeTypePrimitive.Boolean;
        public override Value ConvertTo(AttributeType to)
        {
            return to.PrimaryType switch
            {
                PrimaryType.Boolean => (Value) this,
                PrimaryType.String => (Value == null ? ValueString.NullString : new ValueString(Value.ToString())),
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }
        public override Value GetDefaultValue() => new ValueBoolean(false);

        public override Value IsLowerThan(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return new ValueBoolean(!Value.Ref && ((ValueBoolean) value).Value.Ref);
        }
        
        public override Value And(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return new ValueBoolean(Value.Ref && ((ValueBoolean) value).Value.Ref);
        }
        
        public override Value Or(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return new ValueBoolean(Value.Ref || ((ValueBoolean) value).Value.Ref);
        }

        public override Value Negate() => new ValueBoolean(IsNull ? null : (!Value).Value.ToNullableWrapper());
    }
}