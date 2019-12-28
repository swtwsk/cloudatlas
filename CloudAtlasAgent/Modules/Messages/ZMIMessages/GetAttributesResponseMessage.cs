using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetAttributesResponseMessage : IZMIResponseMessage<AttributesMap>
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIGetAttributesResponse;
        public IZMIRequestMessage Request { get; private set; }
        public AttributesMap Response { get; private set; }
        
        private GetAttributesResponseMessage() {}

        public GetAttributesResponseMessage(IModule source, IModule destination, IZMIRequestMessage request,
            AttributesMap response)
        {
            Source = source;
            Destination = destination;
            Request = request;
            Response = response;
        }
    }
}
