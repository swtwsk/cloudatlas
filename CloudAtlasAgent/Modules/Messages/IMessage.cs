using System;
using Ceras;

namespace CloudAtlasAgent.Modules.Messages
{
    public interface IMessage
    {
        Type Source { get; }
        Type Destination { get; }
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
        ZMIAsk,
        ZMIResponse,
        
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
