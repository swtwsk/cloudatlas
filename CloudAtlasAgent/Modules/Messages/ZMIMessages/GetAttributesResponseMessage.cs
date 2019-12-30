using System;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetAttributesResponseMessage : IZMIResponseMessage<AttributesMap>
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIGetAttributesResponse;
        public IZMIRequestMessage Request { get; private set; }
        public AttributesMap Response { get; private set; }
        
        private GetAttributesResponseMessage() {}

        public GetAttributesResponseMessage(Type source, Type destination, IZMIRequestMessage request,
            AttributesMap response)
        {
            Source = source;
            Destination = destination;
            Request = request;
            Response = response;
        }
    }
}
