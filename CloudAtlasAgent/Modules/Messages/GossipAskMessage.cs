namespace CloudAtlasAgent.Modules.Messages
{
    public class GossipAskMessage : IMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.GossipAsk;
        
        public GossipInnerMessage GossipMessage { get; private set; }
        
        private GossipAskMessage() {}

        public GossipAskMessage(IModule source, IModule destination, GossipInnerMessage gossipMessage)
        {
            Source = source;
            Destination = destination;
            GossipMessage = gossipMessage;
        }
    }
}
