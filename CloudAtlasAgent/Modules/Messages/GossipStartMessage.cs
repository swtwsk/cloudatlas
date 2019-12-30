using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class GossipStartMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.GossipStart;
        
        private GossipStartMessage() {}

        public GossipStartMessage(Type source, Type destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
