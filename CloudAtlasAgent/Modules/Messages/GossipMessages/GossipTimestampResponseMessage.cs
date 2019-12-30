using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    public class GossipTimestampResponseMessage : GossipMessageBase
    {
        public IList<Timestamps> Timestamps { get; private set; }
        
        private GossipTimestampResponseMessage() {}
        
        public GossipTimestampResponseMessage(Guid guid, IList<Timestamps> timestamps) : base(guid)
        {
            Timestamps = timestamps;
        }
    }
}
