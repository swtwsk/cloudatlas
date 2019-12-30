using System;
using Shared;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class UninstallQueryResponseMessage : IZMIResponseMessage<RefStruct<bool>>
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIUninstallQueryResponse;
        public IZMIRequestMessage Request { get; private set; }
        public RefStruct<bool> Response { get; private set; }
        
        private UninstallQueryResponseMessage() {}

        public UninstallQueryResponseMessage(Type source, Type destination, IZMIRequestMessage request,
            RefStruct<bool> response)
        {
            Source = source;
            Destination = destination;
            Request = request;
            Response = response;
        }
    }
}