using MessagePack;

namespace CloudAtlas.Model
{
    [MessagePackObject]
    public class ValueNull : Value
    {
        private static ValueNull _instance = null;
        
        public static ValueNull Instance => _instance ??= new ValueNull();

        private ValueNull() {}
        
        [IgnoreMember] public override AttributeType AttributeType => AttributeTypePrimitive.Null;
        [IgnoreMember] public override bool IsNull => true;
        public override Value ConvertTo(AttributeType to)
        {
            return to.PrimaryType switch
            {
                PrimaryType.String => (Value) ValueString.NullString,
                _ => this
            };
        }

        public override Value GetDefaultValue() => Instance;

        public override Value IsEqual(Value value) => new ValueBoolean(IsNull && value.IsNull);

        public override Value IsLowerThan(Value value) => Equals(value, Instance) ? this : value.IsLowerThan(this);

        public override Value Add(Value value) => Equals(value, Instance) ? this : value.Add(this);

        public override Value Subtract(Value value) => Equals(value, Instance) ? this : value.Subtract(this);

        public override Value Multiply(Value value) => Equals(value, Instance) ? this : value.Multiply(this);

        public override Value Divide(Value value) => Equals(value, Instance) ? this : value.Divide(this);

        public override Value Modulo(Value value) => Equals(value, Instance) ? this : value.Modulo(this);

        public override Value And(Value value) => Equals(value, Instance) ? this : value.And(this);

        public override Value Or(Value value) => Equals(value, Instance) ? this : value.Or(this);

        public override Value RegExpr(Value value) => Equals(value, Instance) ? this : value.RegExpr(this);

        public override Value Negate() => this;

        public override Value ValueSize() => this;
    }
}