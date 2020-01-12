using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    public class GossipTimestampAsk : GossipMessageBase, IReceivedTimestamped, ISendTimestamped
    {
        public IList<Timestamps> Timestamps { get; private set; }
        public IList<string> QueryNames { get; private set; }
        public int Level { get; private set; }
        public ValueContact Contact { get; private set; }
        public ValueTime SendTimestamp { get; private set; }
        public ValueTime ReceiveTimestamp { get; private set; }
        
        private GossipTimestampAsk() {}

        public GossipTimestampAsk(Guid guid, IList<Timestamps> timestamps, IList<string> queryNames, int level, ValueContact contact) 
            : base(guid)
        {
            Timestamps = timestamps;
            QueryNames = queryNames;
            Level = level;
            Contact = contact;
        }
        
        public void SetSendTimestamp(ValueTime timestamp)
        {
            SendTimestamp = timestamp;
        }

        public void SetReceiveTimestamp(ValueTime timestamp)
        {
            ReceiveTimestamp = timestamp;
        }
    }
}
