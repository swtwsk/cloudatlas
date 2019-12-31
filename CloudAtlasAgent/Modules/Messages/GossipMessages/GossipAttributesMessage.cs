using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    using AttributesList = List<(PathName pathName, AttributesMap attributes)>;
    
    public class GossipAttributesMessage : GossipMessageBase
    {
        public AttributesList Attributes { get; private set; }
        public bool IsResponse { get; private set; }

        private GossipAttributesMessage() {}

        public GossipAttributesMessage(Guid guid, AttributesList attributes, bool isResponse)
            : base(guid)
        {
            Attributes = attributes;
            IsResponse = isResponse;
        }
    }
}
