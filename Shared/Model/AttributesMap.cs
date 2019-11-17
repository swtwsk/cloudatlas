using System;
using System.Collections;
using System.Collections.Generic;
using Ceras;

namespace Shared.Model
{
    public class AttributesMap : IEnumerable<KeyValuePair<Attribute, Value>>, ICloneable
    {
        [Include]
        private Dictionary<Attribute, Value> _map = new Dictionary<Attribute,Value>(new Attribute.AttributeComparer());

        private void CheckNulls(Attribute attribute, Value value) {
            if(attribute == null)
                throw new NullReferenceException("The attribute cannot be null.");
            if (value == null)
                throw new NullReferenceException(
                    "The value cannot be null. You may want create a Value object that contains null.");
        }

        public void Add(Attribute attribute, Value value)
        {
            if (ContainsKey(attribute))
                throw new ArgumentException($"Attribute \"{attribute.Name}\" already exists. " +
                                            $"Use method addOrChange(Attribute, Value) instead.");
            CheckNulls(attribute, value);
            _map[attribute] = value;
        }

        public void Add(string name, Value value) => Add(new Attribute(name), value);

        public void Add(KeyValuePair<Attribute, Value> entry) => Add(entry.Key, entry.Value);

        public void Add(AttributesMap attributesMap)
        {
            foreach (var pair in attributesMap._map) 
                Add(pair);
        }

        public void AddOrChange(Attribute attribute, Value value)
        {
            _map[attribute] = value;
            CheckNulls(attribute, value);
        }

        public void AddOrChange(string name, Value value) => AddOrChange(new Attribute(name), value);

        public void AddOrChange(KeyValuePair<Attribute, Value> entry) => Add(entry.Key, entry.Value);

        public void AddOrChange(AttributesMap attributesMap)
        {
            foreach(var pair in attributesMap._map)
                AddOrChange(pair);
        }

        private void CheckAttribute(Attribute attribute)
        {
            if (attribute == null)
                throw new NullReferenceException("The attribute cannot be null");
        }
        
        [Exclude]
        public Value this[Attribute attribute]
        {
            get => Get(attribute);
            set => Add(attribute, value);
        }

        public Value Get(Attribute attribute)
        {
            if (!TryGetValue(attribute, out var value))
                throw new ArgumentException($"Attribute {attribute.Name} does not exist. " +
                                            $"Use method GetOrNull(Attribute) instead");
            return value;
        }

        public Value Get(string name) => Get(new Attribute(name));

        public bool TryGetValue(Attribute attribute, out Value value)
        {
            CheckAttribute(attribute);
            return _map.TryGetValue(attribute, out value);
        }

        public bool TryGetValue(string name, out Value value) => TryGetValue(new Attribute(name), out value);

        public bool ContainsKey(Attribute attribute)
        {
            CheckAttribute(attribute);
            return _map.ContainsKey(attribute);
        }

        public bool Remove(Attribute attribute)
        {
            CheckAttribute(attribute);
            return _map.Remove(attribute);
        }

        public bool Remove(string name) => _map.Remove(new Attribute(name));

        public IEnumerator<KeyValuePair<Attribute, Value>> GetEnumerator() => _map.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public object Clone() => new AttributesMap {this};

        public override string ToString() => _map.ToString();
    }
}