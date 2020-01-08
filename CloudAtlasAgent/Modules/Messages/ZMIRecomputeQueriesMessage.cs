using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class ZMIRecomputeQueriesMessage : IMessage
    {
        public Type Source { get; private set; } = typeof(ZMIModule);
        public Type Destination { get; private set; } = typeof(ZMIModule);
    }
}
