namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class InstallQueryRequestMessage : IZMIRequestMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIInstallQuery;
        
        public string Query { get; private set; }
        
        private InstallQueryRequestMessage() {}

        public InstallQueryRequestMessage(IModule source, IModule destination, string query)
        {
            Source = source;
            Destination = destination;
            Query = query;
        }
    }
}