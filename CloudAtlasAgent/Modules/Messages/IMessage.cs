namespace CloudAtlasAgent.Modules.Messages
{
    public interface IMessage
    {
        IModule Source { get; }
        IModule Destination { get; }
        MessageType MessageType { get; }
    }

    public enum MessageType
    {
        TimerAddCallback,
        TimerRemoveCallback,
    }
}
