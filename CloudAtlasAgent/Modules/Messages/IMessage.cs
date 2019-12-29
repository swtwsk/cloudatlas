using Ceras;

namespace CloudAtlasAgent.Modules.Messages
{
    public interface IMessage
    {
        IModule Source { get; }
        IModule Destination { get; }
        [Exclude] MessageType MessageType { get; }
    }

    // TODO: Is that necessary at all?
    public enum MessageType
    {
        TimerAddCallback,
        TimerRemoveCallback,
        
        CommunicationSend,
        
        GossipStart,
        GossipAsk,
        GossipResponse,
        
        ZMIGetZones,
        ZMIGetAttributes,
        ZMIGetQueries,
        ZMIInstallQuery,
        ZMIUninstallQuery,
        ZMISetAttribute,
        ZMISetContacts,
        ZMIGetZonesResponse,
        ZMIGetAttributesResponse,
        ZMIGetQueriesResponse,
        ZMIInstallQueryResponse,
        ZMIUninstallQueryResponse,
        ZMISetAttributeResponse,
        ZMISetContactsResponse,
    }
}
