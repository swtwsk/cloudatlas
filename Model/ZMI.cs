using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CloudAtlas.Model
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

        public void PrintAttributes(StreamWriter streamWriter)
        {
            foreach (var (key, value) in Attributes)
                streamWriter.WriteLine($"{key} : {value.AttributeType} = {value}");
            streamWriter.WriteLine();
            foreach (var son in Sons)
                son.PrintAttributes(streamWriter);
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

        public override string ToString() => Attributes.TryGetValue("name", out var name)
            ? name.IsNull ? "/" : name.ToString()
            : Attributes.ToString();
    }
}