using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages
{
    using InformationsList = List<(PathName pathName, AttributesMap attributes)>;
    
    public class GossipInnerMessage
    {
        public DateTimeOffset TimeStamp { get; private set; }
        public InformationsList Informations { get; private set; }
        
        private GossipInnerMessage() {}

        public GossipInnerMessage(DateTimeOffset timeStamp, InformationsList informations)
        {
            TimeStamp = timeStamp;
            Informations = informations;
        }
    }
}
