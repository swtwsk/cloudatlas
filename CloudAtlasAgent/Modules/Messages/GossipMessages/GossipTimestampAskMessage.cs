using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    public class GossipTimestampAskMessage : GossipMessageBase
    {
        public IList<Timestamps> Timestamps { get; private set; }
        public int Level { get; private set; }
        public ValueContact Contact { get; private set; }
        
        private GossipTimestampAskMessage() {}

        public GossipTimestampAskMessage(Guid guid, IList<Timestamps> timestamps, int level, ValueContact contact) 
            : base(guid)
        {
            Timestamps = timestamps;
            Level = level;
            Contact = contact;
        }
    }
}
