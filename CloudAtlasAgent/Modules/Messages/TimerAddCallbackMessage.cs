using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class TimerAddCallbackMessage : IMessage
    {
        public IModule Source { get; }
        public IModule Destination { get; }
        public MessageType MessageType { get; }
        
        public int RequestId { get; }
        public int Delay { get; }
        public DateTimeOffset TimeFrom { get; }
        public Action Callback { get; }

        public TimerAddCallbackMessage(IModule source, IModule destination, int requestId, int delay,
            DateTimeOffset timeFrom, Action callback)
        {
            Source = source;
            Destination = destination;
            MessageType = MessageType.TimerAddCallback;

            RequestId = requestId;
            Delay = delay;
            TimeFrom = timeFrom;
            Callback = callback;
        }
    }
}
