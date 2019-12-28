using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class SetContactsRequestMessage : IZMIRequestMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.ZMISetContacts;
        
        public ValueSet Contacts { get; private set; }
        
        private SetContactsRequestMessage() {}

        public SetContactsRequestMessage(IModule source, IModule destination, ValueSet contacts)
        {
            Source = source;
            Destination = destination;
            Contacts = contacts;
        }
    }
}