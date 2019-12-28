﻿using System.Collections.Generic;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetQueriesResponseMessage : IZMIResponseMessage<HashSet<string>>
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIGetQueriesResponse;
        public IZMIRequestMessage Request { get; private set; }
        public HashSet<string> Response { get; private set; }
        
        private GetQueriesResponseMessage() {}

        public GetQueriesResponseMessage(IModule source, IModule destination, IZMIRequestMessage request,
            HashSet<string> response)
        {
            Source = source;
            Destination = destination;
            Request = request;
            Response = response;
        }
    }
}