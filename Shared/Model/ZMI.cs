using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared.Monads;
using Shared.Parsers;

namespace Shared.Model
{
    public sealed class ZMI : ICloneable
    {
        public AttributesMap Attributes { get; private set; } = new AttributesMap();
        public List<ZMI> Sons { get; private set; } = new List<ZMI>();
        public ZMI Father { get; set; }
        
        public ZMI() : this(null) {}

        public ZMI(ZMI father)
        {
            Father = father;
        }

        public static ZMI FromPathName(string pathName)
        {
            var root = new ZMI();

            var paths = pathName.Split("/");

            if (paths[0] == string.Empty)
                paths = paths.Skip(1).ToArray();

            var current = root;
            var level = 0;
            root.AddBasicAttributes(string.Empty, pathName, level);
            
            foreach (var pathPart in paths)
            {
                var newZmi = new ZMI(current);
                current.AddSon(newZmi);
                
                level++;
                newZmi.AddBasicAttributes(pathPart, pathName, level);
                
                current = newZmi;
            }

            return root;
        }

        private void AddBasicAttributes(string name, string ownerName, int level)
        {
            Attributes.Add("name", new ValueString(name));
            Attributes.Add("level", new ValueInt(level));
            Attributes.Add("owner", new ValueString(ownerName));
            Attributes.Add("contacts", new ValueSet(AttributeTypePrimitive.Contact));
        }

        public void AddSon(ZMI son) => Sons.Add(son);
        public void RemoveSon(ZMI son) => Sons.Remove(son);
        
        private string Name => Father == null ? string.Empty : ((ValueString) Attributes.Get("name")).Value;
        
        public PathName PathName => Father == null ? PathName.Root : Father.PathName.LevelDown(Name);

        public override bool Equals(object obj)
        {
            if (!(obj is ZMI otherZmi))
                return false;

            return otherZmi.PathName.Equals(PathName);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PathName, Father);
        }

        public void UpdateZMI(List<(PathName, AttributesMap)> updates, ValueDuration delay)
        {
            var father = GetFather();
            //updates.Sort((tuple1, tuple2) => tuple1.Item1.CompareTo(tuple2.Item1));
            foreach (var (pathName, attributes) in updates)
            {
                var strPathName = pathName.ToString();
                if (!father.TrySearch(strPathName, out var toUpdate))
                {
                    if (!ZMIParser.TryParseZoneLine(strPathName, father, out toUpdate))
                    {
                        Logger.Logger.LogError($"Could not parse {strPathName}");
                        continue;
                    }
                }

                var hasTimestamp = toUpdate.Attributes.TryGetValue("update", out var timestamp);

                if (hasTimestamp && !delay.IsNull)
                    timestamp = timestamp.Add(delay);

                // do not update already fresher zmis
                if (hasTimestamp &&
                    attributes.TryGetValue("update", out var otherTimeStamp) &&
                    ((ValueTime) timestamp).CompareTo((ValueTime) otherTimeStamp) > 0)
                {
                    continue;
                }

                toUpdate.Attributes = attributes;
            }

            UpdateContacts(father, out _);
        }

        private static void UpdateContacts(ZMI zmi, out ValueSet contacts)
        {
            var result = new ValueSet(AttributeTypePrimitive.Contact);
            
            foreach (var son in zmi.Sons)
            {
                UpdateContacts(son, out var sonContacts);
                result.UnionWith(sonContacts);
            }

            if (zmi.Attributes.TryGetValue("contacts", out var attrContacts) && attrContacts is ValueSet zmiContacts)
                result.UnionWith(zmiContacts);
            
            zmi.Attributes.AddOrChange("contacts", result);

            contacts = result;
        }

        public ZMI GetFather()
        {
            var currentZmi = this;
            while (currentZmi.Father != null)
                currentZmi = currentZmi.Father;
            return currentZmi;
        }

        public long GetLevel() => Attributes.TryGetValue("level", out var lvl) ? ((ValueInt) lvl).Value.Ref : -1;

        public List<Timestamps> AggregateTimeStampsFrom(int level)
        {
            var result = new List<Timestamps>();
            var currentZmi = this;
            
            while (currentZmi.Attributes.TryGetValue("level", out var lvl) && ((ValueInt) lvl).Value.Ref > level)
            {
                currentZmi = currentZmi.Father;
            }
            while (currentZmi.Attributes.TryGetValue("level", out var lvl) && ((ValueInt) lvl).Value.Ref > 0)
            {
                result.Add(currentZmi.Attributes.TryGetValue("update", out var timestamp)
                    ? new Timestamps(currentZmi.PathName, (ValueTime) timestamp)
                    : new Timestamps(currentZmi.PathName, null));
                currentZmi = currentZmi.Father;
            }

            return result;
        }

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

            sb.AppendLine(PathName.ToString());
            foreach (var (key, value) in Attributes)
                sb.AppendLine($"    {key} : {value.AttributeType} = {value}");
            sb.AppendLine();
            foreach (var son in Sons)
                sb.Append(son.PrintAttributes());

            return sb.ToString();
        }

        public void ApplyForEach(Action<ZMI> func)
        {
            func(this);
            foreach (var son in Sons)
                son.ApplyForEach(func);
        }

        public void ApplyUpToFather(Action<ZMI> func)
        {
            func(this);
            Father?.ApplyUpToFather(func);
        }

        public void PurgeTime(ValueTime purgeMoment) => PurgeTime(purgeMoment, out _);
        
        public void PurgeTime(ValueTime purgeMoment, out ISet<ValueContact> contactsToRemove)
        {
            var sonsToRemove = new HashSet<ZMI>();
            contactsToRemove = new HashSet<ValueContact>();

            foreach (var son in Sons)
            {
                son.PurgeTime(purgeMoment, out var sonContactsToRemove);
                contactsToRemove.UnionWith(sonContactsToRemove);

                // TODO: think about it, what about ZMIs without update attr?
                if (!son.Attributes.TryGetValue("update", out var updateVal) || !(updateVal is ValueTime update))
                {
                    Logger.Logger.LogError($"No attribute named update in ZMI {son}");
                    continue;
                }

                if (((ValueBoolean) update.IsLowerThan(purgeMoment)).Value.Ref)
                    sonsToRemove.Add(son);
            }

            foreach (var toRemove in sonsToRemove)
            {
                PurgeSon(toRemove, out var sonContactToRemove);
                if (sonContactToRemove.HasValue)
                    contactsToRemove.Add(sonContactToRemove.Val);
            }

            if (Attributes.TryGetValue("contacts", out var currentContacts) &&
                currentContacts is ValueSet currentContactsSet &&
                currentContactsSet.AttributeType.IsCompatible(new AttributeTypeCollection(PrimaryType.Set,
                    AttributeTypePrimitive.Contact)))
            {
                currentContactsSet.ExceptWith(contactsToRemove);
                Attributes.AddOrChange("contacts", currentContactsSet);
            }
        }

        private void PurgeSon(ZMI toPurge, out Maybe<ValueContact> contactToRemove)
        {
            contactToRemove = Maybe<ValueContact>.Nothing;
            
            if (toPurge.Attributes.TryGetValue("isSingleton", out var isSingletonAttr) && !isSingletonAttr.IsNull &&
                isSingletonAttr is ValueBoolean isSingleton && isSingleton.Value.Ref &&
                toPurge.Attributes.TryGetValue("contacts", out var contactsAttr) && !contactsAttr.IsNull &&
                contactsAttr is ValueSet contactsSet && contactsSet.Count == 1)
            {
                contactToRemove = ((ValueContact) contactsSet.First()).Just();
            }

            RemoveSon(toPurge);
        }

        public void PurgeCardinality()
        {
            var sonsToRemove = new HashSet<ZMI>();
            
            foreach (var son in Sons)
            {
                son.PurgeCardinality();
                if (son.Attributes.TryGetValue("cardinality", out var cardinality) && cardinality is ValueInt cardInt &&
                    cardInt.Value.Ref <= 0)
                    sonsToRemove.Add(son);
            }

            foreach (var toRemove in sonsToRemove)
                RemoveSon(toRemove);
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
