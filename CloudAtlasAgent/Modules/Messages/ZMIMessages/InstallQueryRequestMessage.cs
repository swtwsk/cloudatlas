using System;
using Shared.RPC;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class InstallQueryRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        
        public SignedQuery Query { get; private set; }
        
        private InstallQueryRequestMessage() {}

        public InstallQueryRequestMessage(Type source, Type destination, SignedQuery query)
        {
            Source = source;
            Destination = destination;
            Query = query;
        }
    }
}