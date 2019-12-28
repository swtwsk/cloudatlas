namespace CloudAtlasAgent.Modules.Messages
{
    public class TimerRemoveCallbackMessage : IMessage
    {
        public IModule Source { get; private set; }
        public IModule Destination { get; private set; }
        public MessageType MessageType => MessageType.TimerRemoveCallback;
        
        public int RequestId { get; private set; }
        
        private TimerRemoveCallbackMessage() {}

        public TimerRemoveCallbackMessage(IModule source, IModule destination, int requestId)
        {
            Source = source;
            Destination = destination;
            RequestId = requestId;
        }
    }
}