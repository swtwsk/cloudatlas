using System;

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

        private static long ParseDuration(string value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value IsLowerThan(Value value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value Add(Value value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value Subtract(Value value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value Multiply(Value value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value Divide(Value value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value Modulo(Value value)
        {
            // TODO
            throw new NotImplementedException();
        }

        public override Value Negate()
        {
            // TODO
            throw new NotImplementedException();
        }

        public override AttributeType AttributeType => AttributeTypePrimitive.Duration;

        public override Value ConvertTo(AttributeType to)
        {
            throw new System.NotImplementedException();
        }

        public override Value GetDefaultValue() => new ValueDuration(0);
    }
}