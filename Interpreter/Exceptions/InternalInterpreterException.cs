using System;

namespace CloudAtlas.Interpreter.Exceptions
{
    public class InternalInterpreterException : InterpreterException
    {
        public InternalInterpreterException(string message) : base(message) {}

        public InternalInterpreterException(string message, Exception inner) : base(message, inner) {}
    }
}