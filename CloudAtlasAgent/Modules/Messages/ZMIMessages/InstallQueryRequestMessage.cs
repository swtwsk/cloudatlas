using System;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class InstallQueryRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIInstallQuery;
        
        public string Query { get; private set; }
        
        private InstallQueryRequestMessage() {}

        public InstallQueryRequestMessage(Type source, Type destination, string query)
        {
            Source = source;
            Destination = destination;
            Query = query;
        }
    }
}