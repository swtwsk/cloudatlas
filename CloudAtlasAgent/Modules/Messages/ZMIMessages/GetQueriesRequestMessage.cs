using System;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetQueriesRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }

        private GetQueriesRequestMessage() {}
        public GetQueriesRequestMessage(Type source, Type destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}