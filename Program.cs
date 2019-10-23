using System;
using System.IO;
using CloudAtlas.Model;
using MessagePack;
using MessagePack.Resolvers;
using Attribute = CloudAtlas.Model.Attribute;

namespace CloudAtlas
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // ATTRIBUTE
            var attribute = new Attribute("Atrybut1");
            using (var file = File.Create("attribute.bin"))
            {
                MessagePackSerializer.Serialize(file, attribute);
            }

            Attribute newAttribute;
            using (var file = File.OpenRead("attribute.bin"))
            {
                newAttribute = MessagePackSerializer.Deserialize<Attribute>(file);
            }
            Console.WriteLine(newAttribute);
            
            // VALUEINT
            var ten = new ValueInt(10);
            using (var file = File.Create("ten.bin"))
            {
                MessagePackSerializer.Serialize(file, ten, StandardResolverAllowPrivate.Instance);
            }

            Value tenValD;
            using (var file = File.OpenRead("ten.bin"))
            {
                tenValD = MessagePackSerializer.Deserialize<ValueInt>(file, StandardResolverAllowPrivate.Instance);
            }
            Console.WriteLine(tenValD);

            // ZMI
            var zmi = new ZMI();
            zmi.AddSon(new ZMI());
            zmi.Attributes.Add(newAttribute, new ValueString("jeden"));
            zmi.Attributes.Add(new Attribute("dwa"), new ValueString("dwa"));
            zmi.Attributes.Add(new Attribute("data"), new ValueTime("1997/02/15 21:37:14.880"));
            //zmi.Attributes.Add(new Attribute("lista"),
              //  new ValueList(new List<Value> {new ValueInt(1), new ValueInt(2)}, AttributeTypePrimitive.Integer));
            using (var file = File.Create("zmi.bin"))
            {
                var sw = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
                zmi.PrintAttributes(sw);
                MessagePackSerializer.Serialize(file, zmi, StandardResolverAllowPrivate.Instance);
            }

            using (var file = File.OpenRead("zmi.bin"))
            {
                var dZmi = MessagePackSerializer.Deserialize<ZMI>(file);// StandardResolverAllowPrivate.Instance);
                var sw = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
                dZmi.PrintAttributes(sw);
            }
        }
    }
}
