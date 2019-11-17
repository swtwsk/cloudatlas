using System;

namespace Shared.Interpreter.Exceptions
{
    public abstract class InterpreterException : Exception
    {
        protected InterpreterException() {}
        protected InterpreterException(string message) : base(message) {}
        protected InterpreterException(string message, Exception inner) : base(message, inner) {}
    }
}