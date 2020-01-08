using System;
using System.Collections.Generic;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetQueriesResponseMessage : IZMIResponseMessage<HashSet<string>>
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public IZMIRequestMessage Request { get; private set; }
        public HashSet<string> Response { get; private set; }
        
        private GetQueriesResponseMessage() {}

        public GetQueriesResponseMessage(Type source, Type destination, IZMIRequestMessage request,
            HashSet<string> response)
        {
            Source = source;
            Destination = destination;
            Request = request;
            Response = response;
        }
    }
}