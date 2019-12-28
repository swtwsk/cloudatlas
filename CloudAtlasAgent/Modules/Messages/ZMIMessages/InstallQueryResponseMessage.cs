using Shared;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class InstallQueryResponseMessage : IZMIResponseMessage<RefStruct<bool>>
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIInstallQueryResponse;
        public IZMIRequestMessage Request { get; private set; }
        public RefStruct<bool> Response { get; private set; }
        
        private InstallQueryResponseMessage() {}

        public InstallQueryResponseMessage(IModule source, IModule destination, IZMIRequestMessage request,
            RefStruct<bool> response)
        {
            Source = source;
            Destination = destination;
            Request = request;
            Response = response;
        }
    }
}