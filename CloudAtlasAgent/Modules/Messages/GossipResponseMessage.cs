using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class GossipResponseMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.GossipResponse;
        
        public GossipInnerMessage GossipMessage { get; private set; }
        
        private GossipResponseMessage() {}

        public GossipResponseMessage(Type source, Type destination, GossipInnerMessage gossipMessage)
        {
            Source = source;
            Destination = destination;
            GossipMessage = gossipMessage;
        }
    }
}
