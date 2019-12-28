namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetZonesRequestMessage : IZMIRequestMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIGetZones;

        private GetZonesRequestMessage() {}
        
        public GetZonesRequestMessage(IModule source, IModule destination)
        {
            Source = source;
            Destination = destination;
        }

    }
}