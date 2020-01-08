using System;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetZonesRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }

        private GetZonesRequestMessage() {}
        
        public GetZonesRequestMessage(Type source, Type destination)
        {
            Source = source;
            Destination = destination;
        }

    }
}