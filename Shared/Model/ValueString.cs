using System;
using System.Text.RegularExpressions;
using Shared.Model.Exceptions;

namespace Shared.Model
{
    public class ValueString : ValueSimple<string>
    {
        public static ValueString NullString = new ValueString("NULL");
        
        private ValueString() {}
        public ValueString(string value) : base(value) {}

        public override AttributeType AttributeType => AttributeTypePrimitive.String;
        public override Value ConvertTo(AttributeType to)
        {
            switch (to.PrimaryType)
            {
                case PrimaryType.Boolean:
                    return new ValueBoolean(bool.Parse(Value));
                case PrimaryType.Double:
                    return double.TryParse(Value, out var dResult) ? new ValueDouble(dResult) : new ValueDouble(null);
                case PrimaryType.Duration:
                    return new ValueDuration(Value);
                case PrimaryType.Int:
                    return long.TryParse(Value, out var iResult) ? new ValueInt(iResult) : new ValueInt(null);
                case PrimaryType.String:
                    return Value == null ? NullString : this;
                case PrimaryType.Time:
                    try
                    {
                        return new ValueTime(Value);
                    }
                    catch (FormatException)  // TODO: ?
                    {
                        return new ValueTime((RefStruct<long>) null);
                    }
                default:
                    throw new UnsupportedConversionException(AttributeType, to);
            }
        }

        public override Value GetDefaultValue() => new ValueString(string.Empty);

        public override Value IsLowerThan(Value value)
        {
            SameTypesOrThrow(value, Operation.Compare);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return new ValueBoolean(string.Compare(Value, ((ValueString) value).Value, StringComparison.Ordinal) < 0);
        }

        public override Value Add(Value value)
        {
            SameTypesOrThrow(value, Operation.Add);
            if (IsNull || value.IsNull)
                return new ValueString(null);
            return new ValueString(Value + ((ValueString)value).Value);
        }

        public override Value RegExpr(Value value)
        {
            SameTypesOrThrow(value, Operation.RegExpr);
            if (IsNull || value.IsNull)
                return new ValueBoolean(null);
            return new ValueBoolean(Regex.IsMatch(Value, ((ValueString)value).Value));
        }

        public override Value ValueSize() =>
            new ValueInt(Value == null ? null : ((long) Value.Length).ToNullableWrapper());
    }
}