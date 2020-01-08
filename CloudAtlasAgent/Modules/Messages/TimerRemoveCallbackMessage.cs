using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class TimerRemoveCallbackMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        
        public int RequestId { get; private set; }
        
        private TimerRemoveCallbackMessage() {}

        public TimerRemoveCallbackMessage(Type source, Type destination, int requestId)
        {
            Source = source;
            Destination = destination;
            RequestId = requestId;
        }
    }
}