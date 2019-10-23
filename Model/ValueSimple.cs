using MessagePack;

namespace CloudAtlas.Model
{
    [MessagePackObject]
    public abstract class ValueSimple<T> : Value
        where T : class
    {
        private T _value;

        [Key(1)]
        public T Value
        {
            get => GetValue;
            set => SetValue(value);
        }

        [IgnoreMember] protected virtual T GetValue => _value;
        protected virtual void SetValue(T value) => _value = value;
     
        protected ValueSimple() {}
        public ValueSimple(T value)
        {
            Value = value;
        }

        // TODO: HashCode should be immutable
        public override int GetHashCode() => Value.GetHashCode();

        [IgnoreMember] public override bool IsNull => Value == null;

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