using Ceras;
using Shared.Model.Exceptions;

namespace Shared.Model
{
    /// <summary>
    /// A single value stored as an attribute.
    /// </summary>
    public abstract class Value
    {
        protected Value() {}
        
        [Exclude] public abstract AttributeType AttributeType { get; }
        
        [Exclude] public abstract bool IsNull { get; }

        public abstract Value ConvertTo(AttributeType to);
        
        public abstract Value GetDefaultValue();

        protected void SameTypesOrThrow(Value value, Operation operation)
        {
            if (!AttributeType.IsCompatible(value.AttributeType))
                throw new IncompatibleTypesException(AttributeType, value.AttributeType, operation);
        }

        public virtual Value IsEqual(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Equal);

        public virtual Value IsLowerThan(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Compare);
        
        public virtual Value Add(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Add);
        
        public virtual Value Subtract(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Subtract);
        
        public virtual Value Multiply(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Multiply);
        
        public virtual Value Divide(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Divide);
        
        public virtual Value Modulo(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Modulo);
        
        public virtual Value And(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.And);
        
        public virtual Value Or(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Or);
        
        public virtual Value Negate() =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.Negate);
        
        public virtual Value RegExpr(Value value) =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.RegExpr);
        
        public virtual Value ValueSize() =>
            throw new UnsupportedValueOperationException(AttributeType, Operation.ValueSize);
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return ((ValueBoolean) IsEqual((Value) obj)).Value.Ref;
        }

        public override string ToString() => ((ValueString) ConvertTo(AttributeTypePrimitive.String)).Value;
    }
    
    public enum Operation
    {
        Equal, Compare, Add, Subtract, Multiply, Divide, Modulo, And, Or, RegExpr, Negate, ValueSize,
    }
}