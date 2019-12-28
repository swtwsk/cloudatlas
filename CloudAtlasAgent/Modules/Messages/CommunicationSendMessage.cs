using System.Net;

namespace CloudAtlasAgent.Modules.Messages
{
    public class CommunicationSendMessage : IMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.CommunicationSend;
        
        public IMessage MessageToSend { get; private set; }
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }
        
        private CommunicationSendMessage() {}

        public CommunicationSendMessage(IModule source, IModule destination, IMessage messageToSend, IPAddress address, int port)
        {
            Source = source;
            Destination = destination;
            MessageToSend = messageToSend;
            Address = address;
            Port = port;
        }
    }
}
