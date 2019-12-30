using System;
using System.Net;

namespace CloudAtlasAgent.Modules.Messages
{
    public class CommunicationSendMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.CommunicationSend;
        
        public IMessage MessageToSend { get; private set; }
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }
        
        private CommunicationSendMessage() {}

        public CommunicationSendMessage(Type source, Type destination, IMessage messageToSend, IPAddress address, int port)
        {
            Source = source;
            Destination = destination;
            MessageToSend = messageToSend;
            Address = address;
            Port = port;
        }
    }
}
