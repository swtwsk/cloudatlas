using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class TimerAddCallbackMessage : IMessage
    {
        public Type Source { get; private set; }
        public Type Destination { get; private set; } = typeof(TimerModule);
        
        public int RequestId { get; private set; }
        public int Delay { get; private set; }
        public DateTimeOffset TimeFrom { get; private set; }
        public Action Callback { get; private set; }
        
        private TimerAddCallbackMessage() {}

        public TimerAddCallbackMessage(Type source, int requestId, int delay, DateTimeOffset timeFrom, Action callback)
        {
            Source = source;

            RequestId = requestId;
            Delay = delay;
            TimeFrom = timeFrom;
            Callback = callback;
        }
    }
}
