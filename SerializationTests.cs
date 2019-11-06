using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ceras;
using Ceras.Resolvers;
using CloudAtlas.Model;
using Attribute = CloudAtlas.Model.Attribute;

namespace CloudAtlas
{
    public class SerializationTests
    {
        static void OldMain(string[] args)
        {
            Console.WriteLine("Hello World!");

            var buffer = new byte[1024];

            var config = new SerializerConfig
            {
                OnConfigNewType = t =>
                {
                    if (t.Type.IsSubclassOf(typeof(Value)))
                    {
                        t.CustomResolver = (c, tp) => c.Advanced
                            .GetFormatterResolver<DynamicObjectFormatterResolver>()
                            .GetFormatter(tp);
                    }

                    if (t.Type.IsGenericType)
                        if (t.Type.GetGenericTypeDefinition() == typeof(MyClass<>))
                        {
                            t.CustomResolver = (c, tp) => c.Advanced
                                .GetFormatterResolver<DynamicObjectFormatterResolver>()
                                .GetFormatter(tp);
                        }
                }
            };
            var ceras = new CerasSerializer(config);

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

            var t = new AttributeTypeCollection(PrimaryType.List, AttributeTypePrimitive.Boolean);
            using (var file = File.Create("ten.bin"))
            {
                var data = ceras.Serialize(t);
                file.Write(data);
            }
            using (var file = File.OpenRead("ten.bin"))
            {
                file.Read(buffer);
                Console.WriteLine(ceras.Deserialize<AttributeTypeCollection>(buffer));
            }
            
            IList<int> list = new List<int>{1, 2, 3};
            using (var file = File.Create("testList.bin"))
            {
                file.Write(ceras.Serialize(list));
            }
            using (var file = File.OpenRead("testList.bin"))
            {
                file.Read(buffer);
                var dList = ceras.Deserialize<IList<int>>(buffer);
//                foreach (var v in dList)
//                    Console.WriteLine(v);
            }

            var listVal = new List<Value> {new ValueInt(1), new ValueInt(2)};
            var lista = new ValueSet(listVal.ToHashSet(), AttributeTypePrimitive.Integer);
            using (var file = File.Create("lista.bin"))
            {
                file.Write(ceras.Serialize(lista));
            }
            using (var file = File.OpenRead("lista.bin"))
            {
                file.Read(buffer);
                var dList = ceras.Deserialize<ValueSet>(buffer);
                foreach (var s in dList)
                    Console.WriteLine(s);
            }
            
            using (var file = File.Create("lista.bin"))
            {
                var mySet = new MyClass<string>(100);
                ((ISet<string>) mySet).Add("dupa");
                file.Write(ceras.Serialize<MyClass<string>>(mySet));
                Console.WriteLine("  <>  " + mySet.i);
            }
            using (var file = File.OpenRead("lista.bin"))
            {
                file.Read(buffer);
                var dList = ceras.Deserialize<MyClass<string>>(buffer);
                foreach (var s in dList)
                    Console.WriteLine(s);
                Console.WriteLine(dList.i);
            }

            // ZMI
            var zmi = new ZMI();
            zmi.AddSon(new ZMI());
            zmi.Attributes.Add(newAttribute, new ValueString("jeden"));
            zmi.Attributes.Add(new Attribute("dwa"), new ValueString("dwa"));
            zmi.Attributes.Add(new Attribute("data"), new ValueTime("1997/02/15 21:37:14.880"));
            zmi.Attributes.Add(new Attribute("lista"),
                new ValueList(new List<Value> {new ValueInt(1), new ValueInt(2)}, AttributeTypePrimitive.Integer));
            zmi.Attributes.Add(new Attribute("set"),
                new ValueSet(new HashSet<Value> {new ValueBoolean(true), new ValueBoolean(false)},
                    AttributeTypePrimitive.Boolean));
            using (var file = File.Create("zmi.bin"))
            {
                //var sw = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
                //zmi.PrintAttributes(sw);
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

    public class MyClass<T> : ISet<T>
    {
        [Include]
        public int i;
        
        public MyClass()
        {
            _setImplementation = new HashSet<T>();
        }

        public MyClass(int i) : this()
        {
            this.i = i;
        }
        
        [Include] private ISet<T> _setImplementation;
        public IEnumerator<T> GetEnumerator()
        {
            return _setImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _setImplementation).GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            _setImplementation.Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _setImplementation.ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _setImplementation.IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _setImplementation.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _setImplementation.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _setImplementation.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _setImplementation.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _setImplementation.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _setImplementation.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _setImplementation.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _setImplementation.UnionWith(other);
        }

        bool ISet<T>.Add(T item)
        {
            return _setImplementation.Add(item);
        }

        public void Clear()
        {
            _setImplementation.Clear();
        }

        public bool Contains(T item)
        {
            return _setImplementation.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _setImplementation.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _setImplementation.Remove(item);
        }

        public int Count => _setImplementation.Count;

        public bool IsReadOnly => _setImplementation.IsReadOnly;
    }
}