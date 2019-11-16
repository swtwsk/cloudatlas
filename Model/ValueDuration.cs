using System;
using System.Reflection;
using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class ValueDuration : ValueSimple<RefStruct<long>>
    {
        private ValueDuration() {}
        
        public ValueDuration(RefStruct<long> value) : base(value) {}
        
        public ValueDuration(long seconds, long milliseconds) : this(seconds * 1000L + milliseconds) {}

        public ValueDuration(long minutes, long seconds, long milliseconds)
            : this(minutes * 60L + seconds, milliseconds) {}

        public ValueDuration(long hours, long minutes, long seconds, long milliseconds)
            : this(hours * 60L + minutes, seconds, milliseconds) {}

        public ValueDuration(long days, long hours, long minutes, long seconds, long milliseconds)
            : this(days * 24L + hours, minutes, seconds, milliseconds) {}

        public ValueDuration(string value) : this(ParseDuration(value)) {}
        
        public static ValueBoolean operator <(ValueDuration a, ValueDuration b) => new ValueBoolean(a.Value < b.Value);
        public static ValueBoolean operator >(ValueDuration a, ValueDuration b) => new ValueBoolean(a.Value > b.Value);
        public static ValueDuration operator +(ValueDuration a, ValueDuration b) => new ValueDuration(a.Value + b.Value);
        public static ValueDuration operator -(ValueDuration a, ValueDuration b) => new ValueDuration(a.Value - b.Value);
        public static ValueDuration operator *(ValueDuration a, ValueDuration b) => new ValueDuration(a.Value * b.Value);

        private static long ParseDuration(string value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value IsLowerThan(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if(IsNull || value.IsNull)
                return new ValueBoolean(null);
            return this < (value as ValueDuration);
        }

        public override Value Add(Value value)
        {
            if (value.AttributeType.IsCompatible(AttributeTypePrimitive.Time))
            {
                return value.Add(this);
            }
            if (TryOpByConstant(value, value.GetType().GetMethod("Add"), out var toReturn))
                return toReturn;
            
            SameTypesOrThrow(value, Operation.Add);
            if(IsNull || value.IsNull)
                return new ValueDuration((RefStruct<long>) null);
            return this + (value as ValueDuration);
        }

        public override Value Subtract(Value value)
        {
            if (TryOpByConstant(value, value.GetType().GetMethod("Subtract"), out var toReturn))
                return toReturn;
            
            SameTypesOrThrow(value, Operation.Subtract);
            if(IsNull || value.IsNull)
                return new ValueDuration((RefStruct<long>) null);
            return this - (value as ValueDuration);
        }

        public override Value Multiply(Value value)
        {
            if (TryOpByConstant(value, value.GetType().GetMethod("Multiply"), out var toReturn))
                return toReturn;

            SameTypesOrThrow(value, Operation.Multiply);
            if(IsNull || value.IsNull)
                return new ValueDuration((RefStruct<long>) null);
            return this * (value as ValueDuration);
        }

        public override Value Divide(Value value)
        {
            if (TryOpByConstant(value, value.GetType().GetMethod("Divide"), out var toReturn))
                return toReturn;
            
            SameTypesOrThrow(value, Operation.Divide);
            if(value.IsNull)
                return new ValueDouble(null);
            if(((ValueDuration) value).Value == 0L)
                throw new ArithmeticException("Division by zero.");
            return IsNull
                ? new ValueDouble(null)
                : new ValueDouble((double) Value.Ref / ((ValueDuration) value).Value);
        }

        public override Value Modulo(Value value)
        {
            if (TryOpByConstant(value, value.GetType().GetMethod("Modulo"), out var toReturn))
                return toReturn;
            
            SameTypesOrThrow(value, Operation.Modulo);
            if(value.IsNull)
                return new ValueDuration((RefStruct<long>) null);
            if(((ValueDuration)value).Value == 0L)
                throw new ArithmeticException("Division by zero.");
            return IsNull
                ? new ValueDuration((RefStruct<long>) null)
                : new ValueDuration(Value % ((ValueDuration) value).Value);
        }

        public override Value Negate()
        {
            // TODO: Negation of duration?
            return new ValueDuration(IsNull ? null : (-Value.Ref).ToNullableWrapper());
        }
        
        private bool TryOpByConstant(Value value, MethodBase method, out Value outValue)
        {
            outValue = null;
            
            switch (value.AttributeType.PrimaryType)
            {
                case PrimaryType.Int when IsNull || value.IsNull:
                case PrimaryType.Double when IsNull || value.IsNull:
                    outValue = new ValueDuration((RefStruct<long>) null);
                    return true;
                case PrimaryType.Int:
                {
                    var beneath = new ValueInt(Value.Ref);
                    var newVal = method.Invoke(beneath, new object[]{value});
                    var innerRefVal = newVal.GetType().GetProperty("Value")?.GetValue(newVal);
                    var innerVal = innerRefVal?.GetType().GetProperty("Ref")?.GetValue(innerRefVal);
                    outValue = new ValueDuration(Convert.ToInt64(innerVal));
                    return true;
                }
                case PrimaryType.Double:
                {
                    var beneath = new ValueDouble(Value.Ref);
                    var newVal = method.Invoke(beneath, new object[]{value});
                    var innerVal = ((ValueDouble) newVal).Value.Ref;
                    outValue = new ValueDuration(Convert.ToInt64(innerVal));
                    return true;
                }
            }

            return false;
        }

        public override AttributeType AttributeType => AttributeTypePrimitive.Duration;

        public override Value ConvertTo(AttributeType to)
        {
            return to.PrimaryType switch
            {
                PrimaryType.Duration => (Value) this,
                PrimaryType.Int => new ValueInt(Value),
                PrimaryType.String => (Value == null ? ValueString.NullString : new ValueString(Value.ToString())),
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }

        public override Value GetDefaultValue() => new ValueDuration(0L);
    }
}