namespace CloudAtlasAgent.Modules.Messages
{
    public class GossipStartMessage : IMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.GossipStart;
        
        private GossipStartMessage() {}

        public GossipStartMessage(IModule source, IModule destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
