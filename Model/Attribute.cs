using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MessagePack;

namespace CloudAtlas.Model
{
    [MessagePackObject]
    public class Attribute
    {
        [Key(0)] public string Name { get; }

        public static implicit operator Attribute(string name) => new Attribute(name);

        public Attribute(string name)
        {
            if (!Regex.IsMatch(name, "^&?[a-zA-Z]{1}[a-zA-z0-9_]*$"))
                throw new System.ArgumentException("Invalid name: may contain only letters, digits, underscores, "
                                                   + "must start with a letter and may optionally have an ampersand at the beginning.");
            Name = name;
        }

        public static bool IsQuery(Attribute attribute) => attribute.Name.StartsWith("&");

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return Name.Equals(((Attribute) obj).Name);
        }

        public override string ToString() => Name;

        [MessagePackObject]
        public class AttributeComparer : IEqualityComparer<Attribute>
        {
            public bool Equals(Attribute x, Attribute y) => x != null && y != null && x.Name.Equals(y.Name);

            public int GetHashCode(Attribute obj) => obj.GetHashCode();
        }
    }
}