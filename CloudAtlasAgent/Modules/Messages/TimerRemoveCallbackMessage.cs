namespace CloudAtlasAgent.Modules.Messages
{
    public class TimerRemoveCallbackMessage : IMessage
    {
        public IModule Source { get; }
        public IModule Destination { get; }
        public MessageType MessageType { get; }
        
        public int RequestId { get; }

        public TimerRemoveCallbackMessage(IModule source, IModule destination, int requestId)
        {
            Source = source;
            Destination = destination;
            MessageType = MessageType.TimerRemoveCallback;
            RequestId = requestId;
        }
    }
}