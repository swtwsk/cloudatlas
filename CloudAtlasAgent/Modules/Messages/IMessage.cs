using Ceras;

namespace CloudAtlasAgent.Modules.Messages
{
    public interface IMessage
    {
        IModule Source { get; }
        IModule Destination { get; }
        [Exclude] MessageType MessageType { get; }
    }

    public enum MessageType
    {
        TimerAddCallback,
        TimerRemoveCallback,
        CommunicationSend,
    }
}
