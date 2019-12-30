using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class GossipAskMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.GossipAsk;
        
        public GossipInnerMessage GossipMessage { get; private set; }
        
        private GossipAskMessage() {}

        public GossipAskMessage(Type source, Type destination, GossipInnerMessage gossipMessage)
        {
            Source = source;
            Destination = destination;
            GossipMessage = gossipMessage;
        }
    }
}
