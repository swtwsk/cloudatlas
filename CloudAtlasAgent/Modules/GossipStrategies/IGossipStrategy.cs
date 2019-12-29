using Shared.Model;

namespace CloudAtlasAgent.Modules.GossipStrategies
{
    public interface IGossipStrategy
    {
        bool TryGetContact(ZMI zmi, out ValueContact contact);
    }
}
