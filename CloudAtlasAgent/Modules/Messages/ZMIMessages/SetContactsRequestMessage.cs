using System;
using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages.ZMIMessages
{
    public class SetContactsRequestMessage : IZMIRequestMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        
        public ValueSet Contacts { get; private set; }
        
        private SetContactsRequestMessage() {}

        public SetContactsRequestMessage(Type source, Type destination, ValueSet contacts)
        {
            Source = source;
            Destination = destination;
            Contacts = contacts;
        }
    }
}