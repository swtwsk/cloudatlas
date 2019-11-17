using System;

namespace Shared.Model.Exceptions
{
    public class IncompatibleTypesException : NotSupportedException
    {
        public AttributeType Left { get; }
        public AttributeType Right { get; }
        public Operation Operation { get; }

        public IncompatibleTypesException(AttributeType left, AttributeType right, Operation operation) 
            : base($"Incompatible types: {left} and {right} in operation {operation}.")
        {
            Left = left;
            Right = right;
            Operation = operation;
        }
    }
}