using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ceras;
using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class ValueList : ValueSimple<IList<Value>>, IList<Value>
    {
        [Include] private AttributeTypeCollection _type;

        [Include] private IList<Value> List => base.GetValue;

        private ValueList() {}
        public ValueList(IList<Value> value, AttributeType elementType) : this(elementType)
        {
            if (value != null)
                Value = value;
        }

        public ValueList(AttributeType elementType) : base(new List<Value>())
        {
            _type = new AttributeTypeCollection(PrimaryType.List, elementType);
        }

        public override AttributeType AttributeType => _type;
        public override Value ConvertTo(AttributeType to)
        {
            switch (to.PrimaryType)
            {
                case PrimaryType.List:
                    if (_type.IsCompatible(to))
                        return this;
                    throw new UnsupportedConversionException(AttributeType, to);
                case PrimaryType.Set:
                    if (!_type.ElementType.IsCompatible((_type).ElementType))
                        throw new UnsupportedConversionException(AttributeType, to);
                    if (IsNull)
                        return new ValueSet(null, _type.ElementType);
                    var l = new HashSet<Value>();
                    l.Append(this);
                    return new ValueSet(l, _type.ElementType);
                case PrimaryType.String:
                    return Value == null ? ValueString.NullString : new ValueString(Value.ToString());
                default:
                    throw new UnsupportedConversionException(AttributeType, to);
            }
        }

        public override Value GetDefaultValue() => new ValueList(_type.ElementType);

        protected override IList<Value> GetValue => List?.ToImmutableList();
        protected override void SetValue(IList<Value> value)
        {
            if (value == null)
            {
                base.SetValue(null);
            }
            else
            {
                base.SetValue(new List<Value>());
                foreach (var e in List)
                    (this as IList<Value>).Add(e);
            }
        }

        public override Value Add(Value value)
        {
            SameTypesOrThrow(value, Operation.Add);
            if (IsNull || value.IsNull)
                return new ValueList(null, _type.ElementType);
            var result = new List<Value>(Value);
            result.AddRange(((ValueList)value).Value);
            return new ValueList(result, _type.ElementType);
        }

        public override Value ValueSize() => new ValueInt(List?.LongCount());
        
        private void CheckElement(Value element)
        {
            if (element == null)
                throw new System.ArgumentException("If you want to use null, create an object containing null instead.");
            if (!_type.ElementType.IsCompatible(element.AttributeType))
                throw new System.ArgumentException($"This set contains elements of type {_type.ElementType} only. " +
                                            $"Not compatibile with elements of type: {element.AttributeType}");
        }
        
        public IEnumerator<Value> GetEnumerator() => List.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<Value>.Add(Value item) => (List as ICollection<Value>).Add(item);

        public void Clear() => List.Clear();

        public bool Contains(Value item) => List.Contains(item);

        public void CopyTo(Value[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        public bool Remove(Value item) => List.Remove(item);

        [Exclude] public int Count => List?.Count ?? 0;
        [Exclude] public bool IsReadOnly => List?.IsReadOnly ?? false;
        public int IndexOf(Value item) => List.IndexOf(item);

        public void Insert(int index, Value item)
        {
            CheckElement(item);
            List.Insert(index, item);
        }
        
        public void RemoveAt(int index) => List.RemoveAt(index);

        [Exclude]
        public Value this[int index]
        {
            get => List[index];
            set
            {
                CheckElement(value);
                List[index] = value;
            }
        }
    }
}