using System;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class UninstallQueryRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMIUninstallQuery;
        
        public string QueryName { get; private set; }
        
        private UninstallQueryRequestMessage() {}

        public UninstallQueryRequestMessage(Type source, Type destination, string queryName)
        {
            Source = source;
            Destination = destination;
            QueryName = queryName;
        }
    }
}