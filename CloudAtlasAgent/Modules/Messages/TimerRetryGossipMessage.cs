using System;

namespace CloudAtlasAgent.Modules.Messages
{
    public class TimerRetryGossipMessage : IMessage
    {
        public Type Source { get; private set; } = typeof(GossipModule);
        public Type Destination { get; private set; } = typeof(TimerModule);
        
        public Guid Guid { get; private set; }
        public int Delay { get; private set; }
        public DateTimeOffset TimeStamp { get; private set; }
        public int RequestId { get; private set; }
        
        private TimerRetryGossipMessage() {}

        public TimerRetryGossipMessage(Guid guid, int delay, DateTimeOffset timeStamp, int requestId)
        {
            Guid = guid;
            Delay = delay;
            TimeStamp = timeStamp;
            RequestId = requestId;
        }
    }
}
