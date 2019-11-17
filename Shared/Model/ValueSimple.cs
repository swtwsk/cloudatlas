using Ceras;

namespace Shared.Model
{
    public abstract class ValueSimple<T> : Value
        where T : class
    {    
        [Include]
        private T _value;

        [Exclude]
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        protected virtual T GetValue() => _value;
        protected virtual void SetValue(T value) => _value = value;
     
        protected ValueSimple() {}
        public ValueSimple(T value)
        {
            Value = value;
        }

        // TODO: HashCode should be immutable
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override bool IsNull => Value == null;

        public override Value IsEqual(Value value)
        {
            SameTypesOrThrow(value, Operation.Equal);
            if (IsNull && value.IsNull)
                return new ValueBoolean(true);
            if (IsNull || value.IsNull)
                return new ValueBoolean(false);
            return new ValueBoolean(Value.Equals(((ValueSimple<T>)value).Value));
        }
    }
}