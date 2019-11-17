using Shared.Model;

namespace Shared.Interpreter.Exceptions
{
    public class InvalidTypeException : InterpreterException
    {
        public AttributeType Expected { get; }
        public AttributeType Got { get; }
        
        public InvalidTypeException(AttributeType expected, AttributeType got) : base(
            $"Invalid type. Expected {expected}, got {got}.")
        {
            Expected = expected;
            Got = got;
        }
    }
}