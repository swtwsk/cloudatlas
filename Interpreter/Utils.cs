using Shared.Interpreter.Exceptions;
using Shared.Model;

namespace Interpreter
{
    public static class ValueUtils
    {
        public static bool GetBoolean(this Value value)
        {
            if (!value.AttributeType.IsCompatible(AttributeTypePrimitive.Boolean))
                throw new InvalidTypeException(AttributeTypePrimitive.Boolean, value.AttributeType);

            return (value as ValueBoolean)?.Value?.Ref ?? false;
        }
    }
}