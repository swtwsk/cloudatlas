namespace Shared.Interpreter.Exceptions
{
    public class NoSuchAttributeException : InterpreterException
    {
        private string Attribute { get; }

        public NoSuchAttributeException(string attribute) : base($"Attribute {attribute} does not exist.")
        {
            Attribute = attribute;
        }
    }
}