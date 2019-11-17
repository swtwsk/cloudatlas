using System;

namespace Shared.Model.Exceptions
{
    public class UnsupportedValueOperationException : NotSupportedException
    {
        public AttributeType Left { get; }
        public Operation Operation { get; }

        public UnsupportedValueOperationException(AttributeType left, Operation operation)
            : base($"Type: {left} does not provide operation {operation}.")
        {
            Left = left;
            Operation = operation;
        }
    }
}