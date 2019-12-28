namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class UninstallQueryRequestMessage : IZMIRequestMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIUninstallQuery;
        
        public string QueryName { get; private set; }
        
        private UninstallQueryRequestMessage() {}

        public UninstallQueryRequestMessage(IModule source, IModule destination, string queryName)
        {
            Source = source;
            Destination = destination;
            QueryName = queryName;
        }
    }
}