using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CloudAtlas.Model.Exceptions;

namespace CloudAtlas.Model
{
    public class PathName
    {
        public static PathName Root = new PathName("/");

        private List<string> _components;
        public IList<string> Components => _components.ToImmutableList();
        public string Name { get; }

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

        public string SingletonName
        {
            get
            {
                try
                {
                    return _components[_components.Count - 1];
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new InvalidOperationException("SingletonName is not supported for the root zone");
                }
            }
        }

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return Name.Equals(((PathName)obj).Name);
        }

        public override string ToString() => Name.Equals("") ? "/" : Name;
    }
}