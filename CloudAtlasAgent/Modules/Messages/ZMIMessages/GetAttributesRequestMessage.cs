using System;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetAttributesRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        
        public string PathName { get; private set; }

        private GetAttributesRequestMessage() {}
        
        public GetAttributesRequestMessage(Type source, Type destination, string pathName)
        {
            Source = source;
            Destination = destination;
            PathName = pathName;
        }
    }
}