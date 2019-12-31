using System;
using System.Globalization;
using Shared.Model.Exceptions;
using TimeZoneConverter;

namespace Shared.Model
{
    public class ValueTime : ValueSimple<RefStruct<long>>, IComparable, IComparable<ValueTime>
    {
        private const string ReadTimeFormat = "yyyy/M/dd H:m:ss.fff";
        private const string PrintTimeFormat = "yyyy/MM/dd HH:mm:ss.fff";
        
        private ValueTime() {}
        public ValueTime(RefStruct<long> value) : base(value) {}
        public ValueTime(long value) : base(value) {}
        public ValueTime(string time) : this(DateTimeOffset
            .ParseExact(time, ReadTimeFormat, null, DateTimeStyles.AssumeUniversal)
            .UtcToCest()
            .ToUnixTimeMilliseconds()) {}
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
                    : new ValueString(
                        TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(Value.Ref), TimeUtils.CstZone)
                            .ToString(PrintTimeFormat, CultureInfo.InvariantCulture)
                    )),
                PrimaryType.Time => this,
                _ => throw new UnsupportedConversionException(AttributeType, to)
            };
        }

        public override Value GetDefaultValue() => new ValueTime(0L);
        public int CompareTo(object obj) => obj == null ? 1 :
            obj is ValueTime other ? CompareTo(other) :
            throw new ArgumentException("obj is not the same type as instance");
        
        public int CompareTo(ValueTime other)
        {
            var isOtherNull = other == null || other.IsNull;
            
            if (IsNull)
                return isOtherNull ? 0 : -1;

            if (isOtherNull)
                return 1;
            
            var isLower = ((ValueBoolean) IsLowerThan(other)).Value.Ref;
            var isEqual = ((ValueBoolean) IsEqual(other)).Value.Ref;

            return isEqual ? 0 : isLower ? -1 : 1;
        }
    }

    public static class TimeUtils
    {
        public static TimeZoneInfo CstZone => TimeZoneInfo.FindSystemTimeZoneById(
            Environment.OSVersion.Platform == PlatformID.Win32NT
                ? TZConvert.IanaToWindows("Europe/London")
                : "Europe/London");
        
        public static DateTimeOffset UtcToCest(this DateTimeOffset original)
        {
            var cetTime = TimeZoneInfo.ConvertTime(original, CstZone);
            return original
                .Subtract(cetTime.Offset)
                .ToOffset(cetTime.Offset);
        }
    }
}