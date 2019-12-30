using System;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    public class GossipStartMessage : IMessage
    {
        public Type Source { get; private set; } = typeof(GossipModule);
        public Type Destination { get; private set; } = typeof(GossipModule);
        public MessageType MessageType => MessageType.GossipStart;
    }
}
