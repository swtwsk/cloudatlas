namespace CloudAtlasAgent.Modules.Messages
{
    public class GossipResponseMessage : IMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.GossipResponse;
        
        public GossipInnerMessage GossipMessage { get; private set; }
        
        private GossipResponseMessage() {}

        public GossipResponseMessage(IModule source, IModule destination, GossipInnerMessage gossipMessage)
        {
            Source = source;
            Destination = destination;
            GossipMessage = gossipMessage;
        }
    }
}
