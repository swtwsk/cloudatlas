using CloudAtlas.Model;

namespace CloudAtlas.Interpreter
{
    public class QueryResult {
        public Attribute Name { get; }
        public Value Value { get; }

        public QueryResult(Attribute name, Value value) {
            Name = name;
            Value = value;
        }

        public QueryResult(Value value) : this(null, value) {}

        public override string ToString() => $"{Name}: {Value}";
    }

}