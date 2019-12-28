namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class GetQueriesRequestMessage : IZMIRequestMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIGetQueries;

        private GetQueriesRequestMessage() {}
        public GetQueriesRequestMessage(IModule source, IModule destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}