using System;
using System.Collections.Generic;
using System.Linq;
using Shared;
using Shared.Model;

namespace CloudAtlasAgent.Modules.GossipStrategies
{
    public class RandomGossipStrategy : IGossipStrategy
    {
        private readonly Random _random = new Random();

        public bool TryGetContact(ZMI zmi, out ValueContact contact)
        {
            contact = null;

            if (!zmi.Attributes.TryGetValue("level", out var level))
                throw new ArgumentException($"Could not find `level` in given zmi {zmi}");

            var randomZoneLevels = Enumerable.Range(1, (int) ((ValueInt) level).Value.Ref - 1)
                .ToList();
            randomZoneLevels.Shuffle();
            
            foreach (var rnd in randomZoneLevels)
            {
                var currentZmi = zmi;
                while (currentZmi.Attributes.TryGetValue("level", out var currLevel) &&
                       ((ValueInt) currLevel).Value.Ref != rnd)
                {
                    currentZmi = currentZmi.Father;
                }

                var currFather = currentZmi.Father;
                var otherSons = currFather.Sons.Where(z => z != currentZmi).ToList();
                var randomOtherSonsIndexes = Enumerable.Range(0, otherSons.Count).ToList();
                randomOtherSonsIndexes.Shuffle();

                foreach (var sibling in randomOtherSonsIndexes.Select(i => otherSons[i]))
                {
                    IList<Value> contacts;
                    if (!sibling.Attributes.TryGetValue("contacts", out var contactsAttr) || 
                        contactsAttr.IsNull || (contacts = ((ValueSet) contactsAttr).ToList()).Count == 0)
                        continue;

                    int randomIndex;
                    lock (_random)
                        randomIndex = _random.Next(contacts.Count + 1);
                    contact = (ValueContact) contacts[randomIndex]
                        .ConvertTo(AttributeTypePrimitive.Contact);
                    return true;
                }
            }

            return false;
        }
    }
}