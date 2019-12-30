using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class ZMIAskMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIAsk;
        
        private ZMIAskMessage() {}

        public ZMIAskMessage(Type source, Type destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}
