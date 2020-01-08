using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    using AttributesList = List<(PathName pathName, AttributesMap attributes)>;
    
    public class GossipAttributesMessage : GossipMessageBase
    {
        public AttributesList Attributes { get; private set; }
        public ValueDuration Delay { get; private set; }  // delay is computed once, in the gossip initializer node
        public bool IsResponse { get; private set; }

        private GossipAttributesMessage() {}

        public GossipAttributesMessage(Guid guid, AttributesList attributes, ValueDuration delay, bool isResponse)
            : base(guid)
        {
            Attributes = attributes;
            Delay = delay;
            IsResponse = isResponse;
        }
    }
}
