using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;

namespace CloudAtlas.Model
{
    [MessagePackObject]
    public class ZMI : ICloneable
    {
        [Key(1)]
        public AttributesMap Attributes { get; } = new AttributesMap();
        [Key(2)]
        public List<ZMI> Sons { get; } = new List<ZMI>();
        [Key(3)]
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

        public override string ToString() => Attributes.ToString();
    }
}