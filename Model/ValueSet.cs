using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CloudAtlas.Model.Exceptions;
using MessagePack;

namespace CloudAtlas.Model
{
    [MessagePackObject]
    public class ValueSet : ValueSimple<ISet<Value>>, ISet<Value>
    {
        private readonly AttributeTypeCollection _type;
        private ISet<Value> Set => base.GetValue;

        public ValueSet(AttributeType elementType) : base(new HashSet<Value>())
        {
            _type = new AttributeTypeCollection(PrimaryType.Set, elementType);
        }
        
        public ValueSet(ISet<Value> value, AttributeType elementType) : this(elementType)
        {
            if (value != null)
                Value = value;
        }

        [IgnoreMember] public override AttributeType AttributeType => _type;
        public override Value ConvertTo(AttributeType to)
        {
            switch (to.PrimaryType)
            {
                case PrimaryType.List:
                    if (!_type.ElementType.IsCompatible(((AttributeTypeCollection) to).ElementType))
                        throw new UnsupportedConversionException(AttributeType, to);
                    
                    if (IsNull)
                        return new ValueList(null, _type.ElementType);
                    var l = new List<Value>();
                    l.AddRange(this);
                    return new ValueList(l, _type.ElementType);
                case PrimaryType.Set:
                    if (!AttributeType.IsCompatible(to))
                        throw new UnsupportedConversionException(AttributeType, to);
                    return this;
                case PrimaryType.String:
                    if (Value == null)
                        return ValueString.NullString;
                    var sb = new StringBuilder();
                    sb.Append("{");
                    var notFirst = false;
                    foreach (var v in Value)
                    {
                        if (notFirst)
                            sb.Append(", ");
                        else
                            notFirst = true;
                        sb.Append(v);
                    }
                    sb.Append("}");
                    return new ValueString(sb.ToString());
                default:
                    throw new UnsupportedConversionException(AttributeType, to);
            }
        }

        public override Value GetDefaultValue() => new ValueSet(((AttributeTypeCollection)AttributeType).ElementType);

        protected override ISet<Value> GetValue => Set?.ToImmutableHashSet();
        protected override void SetValue(ISet<Value> value)
        {
            if (Set == null)
            {
                base.SetValue(null);
            }
            else
            {
                base.SetValue(new HashSet<Value>());
                foreach (var e in Set)
                    (this as ISet<Value>).Add(e);
            }
        }

        public override Value Add(Value value)
        {
            SameTypesOrThrow(value, Operation.Add);
            if (IsNull || value.IsNull)
                return new ValueSet(null, ((AttributeTypeCollection)AttributeType).ElementType);
            var result = new HashSet<Value>(((ValueSet)Value).Value);
            return new ValueSet(result, ((AttributeTypeCollection)AttributeType).ElementType);
        }

        public override Value ValueSize() => Set == null ? null : new ValueInt(Set.LongCount());

        private void CheckElement(Value element)
        {
            if (element == null)
                throw new System.ArgumentException("If you want to use null, create an object containing null instead.");
            if (!_type.ElementType.IsCompatible(element.AttributeType))
                throw new System.ArgumentException($"This set contains elements of type {_type.ElementType} only. " +
                                            $"Not compatibile with elements of type: {element.AttributeType}");
        }

        public IEnumerator<Value> GetEnumerator() => Set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<Value>.Add(Value item)
        {
            CheckElement(item);
            (Set as ICollection<Value>).Add(item);
        }

        public void ExceptWith(IEnumerable<Value> other) => Set.ExceptWith(other);

        public void IntersectWith(IEnumerable<Value> other) => Set.IntersectWith(other);

        public bool IsProperSubsetOf(IEnumerable<Value> other) => Set.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<Value> other) => Set.IsProperSubsetOf(other);

        public bool IsSubsetOf(IEnumerable<Value> other) => Set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<Value> other) => Set.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<Value> other) => Set.Overlaps(other);

        public bool SetEquals(IEnumerable<Value> other) => Set.SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<Value> other) => Set.SymmetricExceptWith(other);

        public void UnionWith(IEnumerable<Value> other) => Set.UnionWith(other);

        bool ISet<Value>.Add(Value item)
        {
            CheckElement(item);
            return Set.Add(item);
        }

        public void Clear() => Set.Clear();

        public bool Contains(Value item) => Set.Contains(item);

        public void CopyTo(Value[] array, int arrayIndex) => Set.CopyTo(array, arrayIndex);

        public bool Remove(Value item) => Set.Remove(item);

        [IgnoreMember] public int Count => Set.Count;
        [IgnoreMember] public bool IsReadOnly => Set.IsReadOnly;
    }
}