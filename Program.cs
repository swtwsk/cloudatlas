using System;
using System.Collections.Generic;
using System.IO;
using Ceras;
using CloudAtlas.Model;
using Attribute = CloudAtlas.Model.Attribute;

namespace CloudAtlas
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var buffer = new byte[1024];
//            SerializerConfig config = new SerializerConfig();
//            config.DefaultTargets = TargetMember.PublicProperties;

//            var attType = typeof(Attribute);
//            var types = new Type[] {typeof(string)};
//            var info = attType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
//                CallingConventions.HasThis, types, null);
//            config.ConfigType<Attribute>().ConstructBy(info);
            var ceras = new CerasSerializer();

            // ATTRIBUTE
            var attribute = new Attribute("Atrybut1");
            using (var file = File.Create("attribute.bin"))
            {
                var data = ceras.Serialize(attribute);
                file.Write(data);
            }

            Attribute newAttribute;
            using (var file = File.OpenRead("attribute.bin"))
            {
                file.Read(buffer);
                newAttribute = ceras.Deserialize<Attribute>(buffer);
            }
            Console.WriteLine($"newAttribute = {newAttribute}");
            
            
            // VALUEINT
            var ten = new ValueInt(10);
            using (var file = File.Create("ten.bin"))
            {
                var data = ceras.Serialize(ten);
                file.Write(data);
            }

            Value tenValD;
            using (var file = File.OpenRead("ten.bin"))
            {
                file.Read(buffer);
                tenValD = ceras.Deserialize<ValueInt>(buffer);
            }
            Console.WriteLine(tenValD);

            
            // ZMI
            var zmi = new ZMI();
            zmi.AddSon(new ZMI());
            zmi.Attributes.Add(newAttribute, new ValueString("jeden"));
            zmi.Attributes.Add(new Attribute("dwa"), new ValueString("dwa"));
            zmi.Attributes.Add(new Attribute("data"), new ValueTime("1997/02/15 21:37:14.880"));
            zmi.Attributes.Add(new Attribute("lista"),
                new ValueList(new List<Value> {new ValueInt(1), new ValueInt(2)}, AttributeTypePrimitive.Integer));
            using (var file = File.Create("zmi.bin"))
            {
                var sw = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
                zmi.PrintAttributes(sw);
                file.Write(ceras.Serialize(zmi));
            }

            using (var file = File.OpenRead("zmi.bin"))
            {
                file.Read(buffer);
                var dZmi = ceras.Deserialize<ZMI>(buffer);
                var sw = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
                dZmi.PrintAttributes(sw);
            }
            
        }
    }
}
