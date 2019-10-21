namespace CloudAtlas.Model
{
    public abstract class ValueSimple<T> : Value
        where T : class
    {
        private T _value;

        public T Value
        {
            get => GetValue;
            protected set => _value = value;
        }

        protected virtual T GetValue => _value;

        public ValueSimple(T value)
        {
            Value = value;
        }

        // TODO: HashCode should be immutable
        public override int GetHashCode() => Value.GetHashCode();

        public override bool IsNull => Value == null;

        public override Value IsEqual(Value value)
        {
            SameTypesOrThrow(value, Operation.Equal);
            if (IsNull && value.IsNull)
                return new ValueBoolean(true);
            if (IsNull || value.IsNull)
                return new ValueBoolean(false);
            return new ValueBoolean(value.Equals(((ValueSimple<T>)value).Value));
        }
    }
}