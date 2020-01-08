using System;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    public abstract class GossipMessageBase : IMessage
    {
        public Type Source { get; protected set; } = typeof(GossipModule);
        public Type Destination { get; protected set; } = typeof(GossipModule);
        
        public Guid Guid { get; protected set; }

        protected GossipMessageBase() {}

        protected GossipMessageBase(Guid guid)
        {
            Guid = guid;
        }
    }
}
