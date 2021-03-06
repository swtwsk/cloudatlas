using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Ceras;

namespace Shared.Model
{
    public class PathName : IComparable, IComparable<PathName>
    {
        public static PathName Root = new PathName("/");

        [Include]
        private List<string> _components;
        [Exclude]
        public IList<string> Components => _components.ToImmutableList();
        
        public string Name { get; private set; }

        private PathName() {}
        public PathName(string name)
        {
            name = name == null || name.Equals("/") ? "" : name.Trim();
            if (!Regex.IsMatch(name, "(/\\w+)*"))
                throw new ArgumentException($"Incorrect fully qualified name: {name}.");
            this.Name = name;
            _components = name.Equals("") ? new List<string>() : name.Substring(1).Split("/").ToList();
        }

        public PathName(IEnumerable<string> components)
        {
            var collection = components.ToList();
            _components = new List<string>(collection);
            if (!collection.Any())
                Name = "";
            else
            {
                var currentName = "";
                foreach (var c in collection)
                {
                    currentName += $"/{c}";
                    if (!Regex.IsMatch(c, "\\w+"))
                        throw new ArgumentException($"Incorrect component {c}");
                }
                Name = currentName;
            }
        }

        public PathName LevelUp()
        {
            var componentsUp = new List<string>(_components);
            if (componentsUp.Any())
                componentsUp.RemoveAt(componentsUp.Count - 1);
            return new PathName(componentsUp);
        }

        public PathName LevelDown(string son) => new PathName($"{Name}/{son}");

        [Exclude]
        public string SingletonName
        {
            get
            {
                try
                {
                    return _components[^1];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException("SingletonName is not supported for the root zone");
                }
            }
        }

        public override int GetHashCode() => Name.GetHashCode();

        public int CompareTo(PathName other) => other == null ? 1 : string.CompareOrdinal(ToString(), other.ToString());

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return Name.Equals(((PathName)obj).Name);
        }

        public override string ToString() => Name.Equals("") ? "/" : Name;

        public int CompareTo(object obj) => obj == null ? 1 :
            obj is PathName other ? CompareTo(other) :
            throw new ArgumentException("obj is not the same type as instance");
    }
}