namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetAttributesRequestMessage : IZMIRequestMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIGetAttributes;
        
        public string PathName { get; private set; }

        private GetAttributesRequestMessage() {}
        
        public GetAttributesRequestMessage(IModule source, IModule destination, string pathName)
        {
            Source = source;
            Destination = destination;
            PathName = pathName;
        }
    }
}