using System;
using System.Collections.Generic;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages
{
    public class ZMIResponseMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIResponse;
        
        public ZMI Zmi { get; private set; }
        public IList<ValueContact> FallbackContacts { get; private set; }
        public Guid RequestGuid { get; private set; }

        private ZMIResponseMessage() {}

        public ZMIResponseMessage(Type source, Type destination, ZMI zmi, IList<ValueContact> fallbackContacts, 
            Guid requestGuid)
        {
            Source = source;
            Destination = destination;
            Zmi = (ZMI) zmi.Clone();
            FallbackContacts = fallbackContacts;
            RequestGuid = requestGuid;
        }
    }
}
