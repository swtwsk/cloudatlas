using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages
{
    public interface IReceivedTimestamped : IMessage
    {
        void SetReceiveTimestamp(ValueTime timestamp);
    }
}
