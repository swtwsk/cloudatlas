using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Shared.Model
{
    public class Attribute
    {
        public string Name { get; private set; }

        public static implicit operator Attribute(string name) => new Attribute(name);

        private Attribute() {}
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

        public class AttributeComparer : IEqualityComparer<Attribute>
        {
            public bool Equals(Attribute x, Attribute y) => x != null && y != null && x.Name.Equals(y.Name);

            public int GetHashCode(Attribute obj) => obj.GetHashCode();
        }
    }
}