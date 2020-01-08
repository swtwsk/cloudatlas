using System;
using Shared.RPC;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class SetAttributeRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        
        public AttributeMessage AttributeMessage { get; private set; }

        private SetAttributeRequestMessage() {}

        public SetAttributeRequestMessage(Type source, Type destination, AttributeMessage attributeMessage)
        {
            Source = source;
            Destination = destination;
            AttributeMessage = attributeMessage;
        }
    }
}