using Shared.Model;

namespace CloudAtlasAgent.Modules.Messages
{
    public interface ISendTimestamped
    {
        void SetSendTimestamp(ValueTime timestamp);
    }
}
