using Shared.RPC;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class SetAttributeRequestMessage : IZMIRequestMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMISetAttribute;
        
        public AttributeMessage AttributeMessage { get; private set; }

        private SetAttributeRequestMessage() {}

        public SetAttributeRequestMessage(IModule source, IModule destination, AttributeMessage attributeMessage)
        {
            Source = source;
            Destination = destination;
            AttributeMessage = attributeMessage;
        }
    }
}