using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Shared.Monads;

namespace Shared.Model
{
    public class ZMI : ICloneable
    {
        public AttributesMap Attributes { get; private set; } = new AttributesMap();
        public List<ZMI> Sons { get; private set; } = new List<ZMI>();
        public ZMI Father { get; set; }
        
        public ZMI() : this(null) {}

        public ZMI(ZMI father)
        {
            Father = father;
        }

        public void AddSon(ZMI son) => Sons.Add(son);
        public void RemoveSon(ZMI son) => Sons.Remove(son);
        
        private string Name => Father == null ? string.Empty : ((ValueString) Attributes.Get("name")).Value;
        
        public PathName PathName => Father == null ? PathName.Root : Father.PathName.LevelDown(Name);

        public bool TrySearch(string pathName, out ZMI zmi)
        {
            var paths = pathName.Split("/");

            if (paths[0] == string.Empty)
                paths = paths.Skip(1).ToArray();
            
            if ((paths.Length == 1 && paths[0] == Name) || paths.Length == 0)
            {
                zmi = this;
                return true;
            }
            return TrySearch(paths, 0, out zmi);
        }

        private bool TrySearch(IReadOnlyList<string> paths, int depth, out ZMI zmi)
        {
            if (paths.Count < depth)
            {
                zmi = null;
                return false;
            }
            
            if (paths.Count == depth && paths[depth - 1] == Name)
            {
                zmi = this;
                return true;
            }
            
            foreach (var son in Sons)
            {
                var found = son.TrySearch(paths, depth + 1, out zmi);
                if (found)
                    return true;
            }

            zmi = null;
            return false;
        }

        public string PrintAttributes()
        {
            var sb = new StringBuilder();
            
            foreach (var (key, value) in Attributes)
                sb.AppendLine($"{key} : {value.AttributeType} = {value}");
            sb.AppendLine();
            foreach (var son in Sons)
                sb.Append(son.PrintAttributes());

            return sb.ToString();
        }
        
        public object Clone()
        {
            var result = new ZMI(Father);
            result.Attributes.Add(Attributes.Clone() as AttributesMap);
            foreach (var sonClone in Sons.Select(son => (ZMI)son.Clone()))
            {
                result.Sons.Add(sonClone);
                sonClone.Father = result;
            }

            return result;
        }

        public override string ToString() => PathName.ToString();
    }
}
