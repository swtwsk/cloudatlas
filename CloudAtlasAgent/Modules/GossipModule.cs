using System;
using System.Collections.Concurrent;
using CloudAtlasAgent.Modules.GossipStrategies;
using CloudAtlasAgent.Modules.Messages;

namespace CloudAtlasAgent.Modules
{
    public class GossipModule : IModule
    {
        private GossipModule() {}
        public GossipModule(IGossipStrategy gossipStrategy = null)
        {
            _gossipStrategy = gossipStrategy ?? new RandomGossipStrategy();
        }

        private GossipModule _voidInstance;
        public IModule VoidInstance => _voidInstance ??= new GossipModule();

        private IGossipStrategy _gossipStrategy;
        
        private BlockingCollection<GossipInnerMessage> _toRespond = new BlockingCollection<GossipInnerMessage>();
        private BlockingCollection<GossipInnerMessage> _responses = new BlockingCollection<GossipInnerMessage>();

        public bool Equals(IModule other) => other is GossipModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as GossipModule);
        public override int GetHashCode() => "Gossip".GetHashCode();

        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case GossipAskMessage gossipAskMessage:
                    break;
                case GossipResponseMessage gossipResponseMessage:
                    break;
                case GossipStartMessage gossipStartMessage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        public void Dispose()
        {
        }
    }
}