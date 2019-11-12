using System;
using System.Globalization;
using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class ValueTime : ValueSimple<RefStruct<long>>
    {
        private const string ReadTimeFormat = "yyyy/M/dd H:m:ss.fff";
        private const string PrintTimeFormat = "yyyy/MM/dd HH:mm:ss.fff";
        
        private ValueTime() {}
        public ValueTime(RefStruct<long> value) : base(value) {}
        public ValueTime(long value) : base(value) {}
        public ValueTime(string time) : this(DateTimeOffset.ParseExact(time, ReadTimeFormat, null, 
            DateTimeStyles.AssumeUniversal).ToUnixTimeMilliseconds()) {}
        public ValueTime(DateTimeOffset dateTimeOffset) : this(dateTimeOffset.ToUnixTimeMilliseconds()) {}

        public override AttributeType AttributeType => AttributeTypePrimitive.Time;

        public override Value IsLowerThan(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return new ValueBoolean(Value < ((ValueTime)value).Value);
        }

        public override Value Add(Value value)
        {
            if (!value.AttributeType.IsCompatible(AttributeTypePrimitive.Duration))
                throw new IncompatibleTypesException(AttributeType, value.AttributeType, Operation.Add);
            if(IsNull || value.IsNull)
                return new ValueTime((RefStruct<long>) null);
            return new ValueTime(Value + ((ValueDuration)value).Value);
        }

        public override Value Subtract(Value value)
        {
            if (value.AttributeType.IsCompatible(AttributeTypePrimitive.Duration))
            {
                if (IsNull || value.IsNull)
                    return new ValueTime((RefStruct<long>) null);
                return new ValueTime(Value - ((ValueDuration) value).Value);
            }

            if (value.AttributeType.IsCompatible(AttributeTypePrimitive.Time))
            {
                if (IsNull || value.IsNull)
                    return new ValueTime((RefStruct<long>) null);
                return new ValueDuration(Value - ((ValueTime) value).Value);
            }

            throw new IncompatibleTypesException(AttributeType, value.AttributeType, Operation.Add);
        }

        public override Value ConvertTo(AttributeType to)
        {
            return to.PrimaryType switch
            {
                PrimaryType.String => (Value) (Value == null
                    ? ValueString.NullString
                    : new ValueString(DateTimeOffset
                        .FromUnixTimeMilliseconds(Value.Ref)
                        .ToString(PrintTimeFormat, CultureInfo.InvariantCulture))),
                PrimaryType.Time => this,
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }

        public override Value GetDefaultValue() => new ValueTime(0L);
    }
}