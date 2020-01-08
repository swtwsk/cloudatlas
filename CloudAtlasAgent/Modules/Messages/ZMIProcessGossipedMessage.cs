using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages
{
    public class ZMIProcessGossipedMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; } = typeof(ZMIModule);
        
        public List<(PathName, AttributesMap)> Gossiped { get; private set; }
        public ValueDuration Delay { get; private set; }
        
        private ZMIProcessGossipedMessage() {}

        public ZMIProcessGossipedMessage(Type source, List<(PathName, AttributesMap)> gossiped, ValueDuration delay)
        {
            Source = source;
            Gossiped = gossiped;
            Delay = delay;
        }
    }
}