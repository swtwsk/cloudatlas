using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class TimerAddCallbackMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; }
        public MessageType MessageType => MessageType.TimerAddCallback;
        
        public int RequestId { get; private set; }
        public int Delay { get; private set; }
        public DateTimeOffset TimeFrom { get; private set; }
        public Action Callback { get; private set; }
        
        private TimerAddCallbackMessage() {}

        public TimerAddCallbackMessage(Type source, Type destination, int requestId, int delay,
            DateTimeOffset timeFrom, Action callback)
        {
            Source = source;
            Destination = destination;

            RequestId = requestId;
            Delay = delay;
            TimeFrom = timeFrom;
            Callback = callback;
        }
    }
}
