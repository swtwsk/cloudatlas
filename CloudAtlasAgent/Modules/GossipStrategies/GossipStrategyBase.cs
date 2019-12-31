using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Shared.Model;

namespace CloudAtlasAgent.Modules.GossipStrategies
{
    public interface IGossipStrategy
    {
        bool TryGetContact(ZMI zmi, out ValueContact contact, out int level);
    }
    
    public abstract class GossipStrategyBase : IGossipStrategy
    {
        protected readonly Random Random = new Random();

        protected abstract int GetZoneIndex(int maxLevel);
        
        public bool TryGetContact(ZMI zmi, out ValueContact contact, out int level)
        {
            contact = null;

            if (!zmi.Attributes.TryGetValue("level", out var attrLevel))
                throw new ArgumentException($"Could not find `level` in given zmi {zmi}");

            var maxLevel = (int) ((ValueInt) attrLevel).Value.Ref;
            level = GetZoneIndex(maxLevel);

            var currentZmi = zmi;
            while (currentZmi.Attributes.TryGetValue("level", out var currLevel) &&
                   ((ValueInt) currLevel).Value.Ref != level)
            {
                currentZmi = currentZmi.Father;
            }

            var currFather = currentZmi.Father;
            var otherSons = currFather.Sons.Where(z => !Equals(z, currentZmi)).ToList();
            var randomOtherSonsIndexes = Enumerable.Range(0, otherSons.Count).ToList();
            randomOtherSonsIndexes.Shuffle();

            foreach (var sibling in randomOtherSonsIndexes.Select(i => otherSons[i]))
            {
                IList<Value> contacts;
                if (!sibling.Attributes.TryGetValue("contacts", out var contactsAttr) ||
                    contactsAttr.IsNull || (contacts = ((ValueSet) contactsAttr).ToList()).Count == 0)
                    continue;

                int randomIndex;
                lock (Random)
                    randomIndex = Random.Next(contacts.Count);
                contact = (ValueContact) contacts[randomIndex]
                    .ConvertTo(AttributeTypePrimitive.Contact);
                
                return true;
            }
            
            return false;
        }
    }
}
