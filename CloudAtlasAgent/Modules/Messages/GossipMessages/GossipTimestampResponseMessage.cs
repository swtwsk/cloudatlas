using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.GossipMessages
{
    public class GossipTimestampResponseMessage : GossipMessageBase, IReceivedTimestamped, ISendTimestamped
    {
        public IList<Timestamps> Timestamps { get; private set; }
        public IList<string> QueryNames { get; private set; }
        public ValueTime RequestSendTimestamp { get; private set; }
        public ValueTime RequestReceiveTimestamp { get; private set; }
        public ValueTime ResponseSendTimestamp { get; private set; }
        public ValueTime ResponseReceiveTimestamp { get; private set; }
        
        private GossipTimestampResponseMessage() {}
        
        public GossipTimestampResponseMessage(Guid guid, IList<Timestamps> timestamps, IList<string> queryNames,
            ValueTime requestSendTimestamp, ValueTime requestReceiveTimestamp) : base(guid)
        {
            Timestamps = timestamps;
            QueryNames = queryNames;
            RequestSendTimestamp = requestSendTimestamp;
            RequestReceiveTimestamp = requestReceiveTimestamp;
        }

        public void SetReceiveTimestamp(ValueTime timestamp)
        {
            ResponseReceiveTimestamp = timestamp;
        }

        public void SetSendTimestamp(ValueTime timestamp)
        {
            ResponseSendTimestamp = timestamp;
        }
    }
}
