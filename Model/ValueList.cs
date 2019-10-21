using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CloudAtlas.Model
{
    public class ValueList : ValueSimple<IList<Value>>, IList<Value>
    {
        private AttributeTypeCollection Type { get; }

        private IEnumerable<Value> List => Value;
        
        public ValueList(List<Value> value, AttributeType elementType) : this(elementType)
        {
            if (value != null)
                Value = value;
        }

        public ValueList(AttributeType elementType) : base(new List<Value>())
        {
            Type = new AttributeTypeCollection(PrimaryType.List, elementType);
        }

        public override AttributeType AttributeType => Type;
        public override Value ConvertTo(AttributeType to)
        {
            throw new System.NotImplementedException();
        }

        public override Value GetDefaultValue() => new ValueList(Type.ElementType);

        protected override IList<Value> GetValue => List?.ToImmutableList();  // TODO: Check it

        public override Value Add(Value value)
        {
            SameTypesOrThrow(value, Operation.Add);
            if (IsNull || value.IsNull)
                return new ValueList(null, Type.ElementType);
            var result = new List<Value>(Value);
            result.AddRange(((ValueList)value).Value);
            return new ValueList(result, Type.ElementType);
        }
        
        // ILIST
        public IEnumerator<Value> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<Value>.Add(Value item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(Value item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(Value[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(Value item)
        {
            throw new System.NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(Value item)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index, Value item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        public Value this[int index]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
}