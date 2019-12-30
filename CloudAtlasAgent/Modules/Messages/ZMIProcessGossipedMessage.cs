using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages
{
    public class ZMIProcessGossipedMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; } = typeof(ZMIModule);
        public MessageType MessageType => MessageType.ZMIAsk;
        
        public IList<(PathName, AttributesMap)> Gossiped { get; private set; }
        
        private ZMIProcessGossipedMessage() {}

        public ZMIProcessGossipedMessage(Type source, IList<(PathName, AttributesMap)> gossiped)
        {
            Source = source;
            Gossiped = gossiped;
        }
    }
}