using System;
using Shared.RPC;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class UninstallQueryRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        
        public UnsignQuery UnsignRequest { get; private set; }
        
        private UninstallQueryRequestMessage() {}

        public UninstallQueryRequestMessage(Type source, Type destination, UnsignQuery unsignRequest)
        {
            Source = source;
            Destination = destination;
            UnsignRequest = unsignRequest;
        }
    }
}