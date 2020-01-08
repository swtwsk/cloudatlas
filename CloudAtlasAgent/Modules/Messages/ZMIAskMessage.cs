using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class ZMIAskMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; } = typeof(ZMIModule);
        
        public Guid Guid { get; private set; }
        
        private ZMIAskMessage() {}

        public ZMIAskMessage(Type source, Guid guid)
        {
            Source = source;
            Guid = guid;
        }
    }
}
