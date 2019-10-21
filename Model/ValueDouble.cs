using System;
using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class ValueDouble : ValueSimple<NullableWrapper<double>>
    {
        public ValueDouble(NullableWrapper<double> value) : base(value) {}

        public override AttributeType AttributeType => AttributeTypePrimitive.Double;
        public override Value ConvertTo(AttributeType to)
        {
            return to.PrimaryType switch
            {
                PrimaryType.Double => this as Value,
                PrimaryType.Int => new ValueInt(Value == null ? null : Convert.ToInt64(Value).ToNullableWrapper()),
                PrimaryType.String => Value == null ? ValueString.NullString : new ValueString(Value.ToString()),
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }

        public override Value GetDefaultValue() => new ValueDouble(0.0);
        
        public static ValueBoolean operator <(ValueDouble a, ValueDouble b) => new ValueBoolean(a.Value < b.Value);
        public static ValueBoolean operator >(ValueDouble a, ValueDouble b) => new ValueBoolean(a.Value > b.Value);
        public static ValueDouble operator +(ValueDouble a, ValueDouble b) => new ValueDouble(a.Value + b.Value);
        public static ValueDouble operator -(ValueDouble a, ValueDouble b) => new ValueDouble(a.Value - b.Value);
        public static ValueDouble operator *(ValueDouble a, ValueDouble b) => new ValueDouble(a.Value * b.Value);
        public static ValueDouble operator /(ValueDouble a, ValueDouble b) => new ValueDouble(a.Value / b.Value);

        public override Value IsLowerThan(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return this < (value as ValueDouble);
        }
        
        public override Value Add(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return this + (value as ValueDouble);
        }
        
        public override Value Subtract(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return this - (value as ValueDouble);
        }
        
        public override Value Multiply(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return this * (value as ValueDouble);
        }
        
        public override Value Divide(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return this / (value as ValueDouble);
        }

        public override Value Negate() => new ValueDouble(IsNull ? null : (-Value.Wrapped).ToNullableWrapper());
    }
}