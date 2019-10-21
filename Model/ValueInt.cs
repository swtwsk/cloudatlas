using System;
using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class ValueInt : ValueSimple<NullableWrapper<long>>
    {
        public ValueInt(NullableWrapper<long> value) : base(value) {}
        public ValueInt(long value) : base(value) {}

        public override AttributeType AttributeType => AttributeTypePrimitive.Integer;
        public override Value ConvertTo(AttributeType to)
        {
            return AttributeType.PrimaryType switch
            {
                PrimaryType.Double => (Value) new ValueDouble(Value == null
                    ? null
                    : Convert.ToDouble(Value).ToNullableWrapper()),
                PrimaryType.Duration => new ValueDuration(Value),
                PrimaryType.Int => this,
                PrimaryType.String => (Value == null ? ValueString.NullString : new ValueString(Value.ToString())),
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }

        public override Value GetDefaultValue() => new ValueInt(0L);


        public static ValueBoolean operator <(ValueInt a, ValueInt b) => new ValueBoolean(a.Value < b.Value);
        public static ValueBoolean operator >(ValueInt a, ValueInt b) => new ValueBoolean(a.Value > b.Value);
        public static ValueInt operator +(ValueInt a, ValueInt b) => new ValueInt(a.Value + b.Value);
        public static ValueInt operator -(ValueInt a, ValueInt b) => new ValueInt(a.Value - b.Value);
        public static ValueInt operator *(ValueInt a, ValueInt b) => new ValueInt(a.Value * b.Value);

        public override Value IsLowerThan(Value value) {
            SameTypesOrThrow(value, Operation.Compare);
            if(IsNull || value.IsNull)
                return new ValueBoolean(null);
            return this < (value as ValueInt);
        }

        public override Value Add(Value value) {
            SameTypesOrThrow(value, Operation.Add);
            if(IsNull || value.IsNull)
                return new ValueInt(null);
            return this + (value as ValueInt);
        }
	
        public override Value Subtract(Value value) {
            SameTypesOrThrow(value, Operation.Subtract);
            if(IsNull || value.IsNull)
                return new ValueInt(null);
            return this - (value as ValueInt);
        }
	
        public override Value Multiply(Value value) {
            if(value.AttributeType.PrimaryType == PrimaryType.Duration)
                return value.Multiply(this);
            SameTypesOrThrow(value, Operation.Multiply);
            if(IsNull || value.IsNull)
                return new ValueInt(null);
            return this * (value as ValueInt);
        }
	
        public override Value Divide(Value value) {
            SameTypesOrThrow(value, Operation.Divide);
            if(value.IsNull)
                return new ValueDouble(null);
            if(((ValueInt) value).Value == 0L)
                throw new ArithmeticException("Division by zero.");
            return IsNull
                ? (Value) new ValueDouble(null)
                : (Value) new ValueDouble((double) Value.Wrapped / ((ValueInt) value).Value);
        }
	
        public override Value Modulo(Value value) {
            SameTypesOrThrow(value, Operation.Modulo);
            if(value.IsNull)
                return new ValueInt(null);
            if(((ValueInt)value).Value == 0L)
                throw new ArithmeticException("Division by zero.");
            return IsNull ? new ValueInt(null) : new ValueInt(Value % ((ValueInt)value).Value);
        }
	
        public override Value Negate() {
            return new ValueInt(IsNull ? null : (-Value.Wrapped).ToNullableWrapper());
        }
    }
}