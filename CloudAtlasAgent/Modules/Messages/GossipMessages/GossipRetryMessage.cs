using System;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    public class GossipRetryMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; } = typeof(GossipModule);
        public MessageType MessageType => MessageType.CommunicationSend;

        public Guid Guid { get; private set; }
        
        private GossipRetryMessage() {}

        public GossipRetryMessage(Type source, Guid guid)
        {
            Source = source;
            Guid = guid;
        }
    }
}
