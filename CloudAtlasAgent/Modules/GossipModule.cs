using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CloudAtlasAgent.Modules.GossipStrategies;
using CloudAtlasAgent.Modules.Messages;
using CloudAtlasAgent.Modules.Messages.GossipMessages;
using Shared;
using Shared.Logger;
using Shared.Model;

namespace CloudAtlasAgent.Modules
{
    public class GossipModule : IModule
    {
        private enum GossipState
        {
            AskTimestamp,
            ResponseTimestamp,
            SendAttributes,
            ResponseAttributes,
        }
        
        private GossipModule() {}

        public GossipModule(IExecutor executor, int gossipTimer, IGossipStrategy gossipStrategy = null)
        {
            _executor = executor;
            _gossipStrategy = gossipStrategy ?? new RandomGossipStrategy();
            _gossipThread = new Thread(Gossip);
            _gossipThread.Start();

            _gossipTimer = gossipTimer;
            _executor.AddMessage(new TimerAddCallbackMessage(GetType(), typeof(TimerModule), _timerMessageId++,
                _gossipTimer, DateTimeOffset.Now, AddGossipTimer));
        }

        private void AddGossipTimer()
        {
            _executor.AddMessage(new TimerAddCallbackMessage(GetType(), typeof(TimerModule), _timerMessageId++,
                _gossipTimer, DateTimeOffset.Now, AddGossipTimer));
            _executor.AddMessage(new GossipStartMessage());
        }

        private readonly int _gossipTimer;
        private int _timerMessageId = 0;

        private GossipModule _voidInstance;
        public IModule VoidInstance => _voidInstance ??= new GossipModule();

        private readonly Thread _gossipThread;

        private readonly IExecutor _executor;
        private readonly IGossipStrategy _gossipStrategy;

        private readonly IDictionary<Guid, (ZMI, IList<ValueContact>)> _zmis =
            new Dictionary<Guid, (ZMI, IList<ValueContact>)>();
        
        private readonly IDictionary<Guid, GossipState> _states = new Dictionary<Guid, GossipState>();

        private readonly IDictionary<Guid, TimestampsInfo> _timestamps = new Dictionary<Guid, TimestampsInfo>();

        private readonly BlockingCollection<(Guid, GossipMessageBase)> _forThread =
            new BlockingCollection<(Guid, GossipMessageBase)>();

        private class TimestampsInfo
        {
            public ValueContact Contact { get; }
            public int Level { get; }
            public IList<Timestamps> MyTimestamps { get; }
            public IList<Timestamps> HisTimestamps { get; set; }

            public TimestampsInfo(ValueContact contact, int level, IList<Timestamps> myTimestamps,
                IList<Timestamps> hisTimestamps)
            {
                Contact = contact;
                Level = level;
                MyTimestamps = myTimestamps;
                HisTimestamps = hisTimestamps;
            }
            
            public void Deconstruct(out ValueContact contact, out int level, out IList<Timestamps> myTimestamps,
                out IList<Timestamps> hisTimestamps)
            {
                contact = Contact;
                level = Level;
                myTimestamps = MyTimestamps;
                hisTimestamps = HisTimestamps;
            }
        }

        public bool Equals(IModule other) => other is GossipModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as GossipModule);
        public override int GetHashCode() => "Gossip".GetHashCode();

        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case ZMIResponseMessage responseMessage:
                    _zmis.TryAdd(responseMessage.RequestGuid, (responseMessage.Zmi, responseMessage.FallbackContacts));
                    _forThread.Add((responseMessage.RequestGuid, null));
                    break;
                case GossipTimestampAskMessage gossipAskMessage:
                    _timestamps.Add(gossipAskMessage.Guid,
                        new TimestampsInfo(gossipAskMessage.Contact, gossipAskMessage.Level, null,
                            gossipAskMessage.Timestamps));
                    _executor.AddMessage(new ZMIAskMessage(GetType(), gossipAskMessage.Guid));
                    break;
                case GossipTimestampResponseMessage gossipResponseMessage:
                    var guid = gossipResponseMessage.Guid;
                    if (!_timestamps.TryGetValue(guid, out var timestamps))
                    {
                        Logger.LogError($"Could not find timestamp infos for guid {guid}");
                        return;
                    }
                    timestamps.HisTimestamps = gossipResponseMessage.Timestamps;
                    _forThread.Add((gossipResponseMessage.Guid, gossipResponseMessage));
                    break;
                case GossipStartMessage _:
                    _executor.AddMessage(new ZMIAskMessage(GetType(), Guid.NewGuid()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        private static (IList<Timestamps>, ValueContact) PrepareGossipTimestampsMessage(ZMI zmi, int level)
        {
            var toSend = zmi.AggregateTimeStampsFrom(level);
            if (!zmi.Attributes.TryGetValue("contacts", out var attrMyContacts) || attrMyContacts.IsNull ||
                !attrMyContacts.AttributeType.IsCompatible(
                    new AttributeTypeCollection(PrimaryType.Set, AttributeTypePrimitive.Contact)) ||
                ((ValueSet) attrMyContacts).Count != 1)
            {
                Logger.LogError("Could not find proper contacts set");
            }

            var myContact = (ValueContact) ((ValueSet) attrMyContacts).First();

            return (toSend, myContact);
        }
        
//        private static GossipAttributesMessage prepareGossipAttributesMessage(ZMI zmi, int level)
//        {
//            var toSend = zmi.AggregateAttributesAbove(level);
//            if (!zmi.Attributes.TryGetValue("contacts", out var attrMyContacts) || attrMyContacts.IsNull ||
//                !attrMyContacts.AttributeType.IsCompatible(
//                    new AttributeTypeCollection(PrimaryType.Set, AttributeTypePrimitive.Contact)) ||
//                ((ValueSet) attrMyContacts).Count != 1)
//            {
//                Logger.LogError("Could not find proper contacts set");
//            }
//
//            var myContact = (ValueContact) ((ValueSet) attrMyContacts).First();
//
//            return new GossipAttributesMessage(DateTimeOffset.Now, toSend, level, myContact);
//        }

        private void Gossip()
        {
            try
            {
                while (true)
                {
                    var (guid, gossipMessage) = _forThread.Take();
                    if (!_zmis.TryGetValue(guid, out var zmiPair))
                    {
                        Logger.LogError($"Could not find zmi for guid {guid}, aborting gossiping then");
                        continue;
                    }

                    var (zmi, fallbacks) = zmiPair;

                    if (gossipMessage != null)
                    {
                        Logger.Log($"Gossip: {gossipMessage}");
//                        ProcessGossip(gossipMessage, zmi);
                        continue;
                    }

                    var timestampsToSave = _timestamps.TryGetValue(guid, out var timestampsInfo)
                        ? RespondTimestamps(guid, timestampsInfo, zmi)
                        : SendInitialTimestamps(guid, zmi, fallbacks);
                    _timestamps[guid] = timestampsToSave;
                }
            }
            catch (ThreadInterruptedException) {}
            catch (Exception e) { Logger.LogException(e); }
        }

        private PathName _currentGossipZMI;  // ?
//        private IDictionary<IGossipInnerMessage, 

        private TimestampsInfo SendInitialTimestamps(Guid guid, ZMI zmi, IList<ValueContact> fallbacks)
        {
            if (!_gossipStrategy.TryGetContact(zmi, out var contact, out var level))
            {
                Logger.Log("RandomOrDefault to do");
                contact = fallbacks.RandomOrDefault();
                if (contact == null)
                {
                    Logger.LogWarning("No contacts at all");
                    // TODO: Multicast here
                    return null;
                }
            }
                    
            Logger.Log($"Sending initial timestamps to {contact}");
            var (myTimestamps, myContact) = PrepareGossipTimestampsMessage(zmi, level);
            var gossipAskMsg = new GossipTimestampAskMessage(guid, myTimestamps, level, myContact);
            
            var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule),
                gossipAskMsg, contact.Address, contact.Port);
            _executor.AddMessage(newMsg);

            return new TimestampsInfo(contact, level, myTimestamps, null);
        }

        private TimestampsInfo RespondTimestamps(Guid guid, TimestampsInfo message, ZMI zmi)
        {
            var (contact, level, _, hisTimestamps) = message;
            
            Logger.Log($"Acquired timestamps from {contact}");

            var (myTimestamps, _) = PrepareGossipTimestampsMessage(zmi, level);
            var gossipResponseMessage = new GossipTimestampResponseMessage(guid, myTimestamps);

            var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule),
                gossipResponseMessage, contact.Address, contact.Port);
            _executor.AddMessage(newMsg);
            
            return new TimestampsInfo(contact, level, myTimestamps, hisTimestamps);
        }

//        private void ProcessGossip(GossipMessageBase gossip, bool isResponse, ZMI zmi)
//        {
//            // TODO:
//            Logger.Log($"Acquired gossip sent at {gossip.TimeStamp} from {gossip.Contact}, isResponse = {isResponse}");
//
//            if (!isResponse)
//            {
//                var innerMessage = PrepareGossipTimestampsMessage(zmi, gossip.Level);
//                var gossipAskMsg = new GossipTimestampResponseMessage(GetType(), typeof(GossipModule), innerMessage);
//                
//                var contact = gossip.Contact;
//                var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule),
//                    gossipAskMsg, contact.Address, contact.Port);
//                _executor.AddMessage(newMsg);
//            }
//        }

        public void Dispose()
        {
            _gossipThread?.Interrupt();
            _forThread?.Dispose();
        }
    }
}