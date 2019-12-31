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
        public GossipModule(IExecutor executor, int gossipTimer, int retryDelay, int maxRetriesCount,
            IGossipStrategy gossipStrategy = null)
        {
            _executor = executor;
            _gossipStrategy = gossipStrategy ?? new RandomGossipStrategy();
            _gossipThread = new Thread(Gossip);
            _gossipThread.Start();

            _gossipTimer = gossipTimer;
            _retryDelay = retryDelay;
            _maxRetriesCount = maxRetriesCount;
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
        private readonly int _retryDelay;
        private readonly int _maxRetriesCount;
        private int _timerMessageId = 0;

        private readonly Thread _gossipThread;

        private readonly IExecutor _executor;
        private readonly IGossipStrategy _gossipStrategy;

        private readonly IDictionary<Guid, (ZMI, IList<ValueContact>)> _zmis =
            new Dictionary<Guid, (ZMI, IList<ValueContact>)>();
        private readonly IDictionary<Guid, TimestampsInfo> _timestamps = new Dictionary<Guid, TimestampsInfo>();
        private readonly IDictionary<Guid, int> _retryIds = new Dictionary<Guid, int>();

        private readonly BlockingCollection<(Guid, GossipMessageBase)> _incomingGossips =
            new BlockingCollection<(Guid, GossipMessageBase)>();

        private class TimestampsInfo
        {
            public ValueContact Contact { get; }
            public int Level { get; }
            public IList<Timestamps> MyTimestamps { get; }
            public IList<Timestamps> HisTimestamps { get; set; }
            
            public int Attempts { get; set; }

            public TimestampsInfo(ValueContact contact, int level, IList<Timestamps> myTimestamps,
                IList<Timestamps> hisTimestamps, int attempts = 1)
            {
                Contact = contact;
                Level = level;
                MyTimestamps = myTimestamps;
                HisTimestamps = hisTimestamps;
                Attempts = attempts;
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

        private void HaltRetry(Guid guid)
        {
            if (_retryIds.TryGetValue(guid, out var removeId))
                _executor.AddMessage(new TimerRemoveCallbackMessage(GetType(), typeof(TimerModule),
                    removeId));

            _retryIds.Remove(guid);
        }

        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case ZMIResponseMessage responseMessage:
                    lock (_zmis)
                        _zmis.TryAdd(responseMessage.RequestGuid,
                            (responseMessage.Zmi, responseMessage.FallbackContacts));
                    _incomingGossips.Add((responseMessage.RequestGuid, null));
                    break;
                case GossipTimestampAskMessage gossipAskMessage:
                    lock (_timestamps)
                        _timestamps[gossipAskMessage.Guid] = new TimestampsInfo(gossipAskMessage.Contact,
                            gossipAskMessage.Level, null, gossipAskMessage.Timestamps);
                    _executor.AddMessage(new ZMIAskMessage(GetType(), gossipAskMessage.Guid));
                    break;
                case GossipTimestampResponseMessage gossipResponseMessage:
                    var guid = gossipResponseMessage.Guid;
                    TimestampsInfo timestamps;
                    lock (_timestamps)
                        if (!_timestamps.TryGetValue(guid, out timestamps))
                        {
                            Logger.LogError($"Could not find timestamp infos for guid {guid}");
                            return;
                        }
                    timestamps.HisTimestamps = gossipResponseMessage.Timestamps;
                    HaltRetry(gossipResponseMessage.Guid);
                    _incomingGossips.Add((gossipResponseMessage.Guid, gossipResponseMessage));
                    break;
                case GossipAttributesMessage gossipAttributesMessage:
                    _incomingGossips.Add((gossipAttributesMessage.Guid, gossipAttributesMessage));
                    break;
                case GossipStartMessage _:
                    var newGuid = Guid.NewGuid();
                    lock (_timestamps)
                        _timestamps[newGuid] = new TimestampsInfo(null, -1, null, null);
                    _executor.AddMessage(new ZMIAskMessage(GetType(), newGuid));

                    var retryId = ++_timerMessageId;
                    _executor.AddMessage(new TimerRetryGossipMessage(newGuid, _retryDelay, DateTimeOffset.Now,
                        retryId));
                    _retryIds.Add(newGuid, retryId);
                    break;
                case GossipRetryMessage retryMessage:
                    TimestampsInfo retryTimestamps;
                    lock (_timestamps)
                        if (!_timestamps.TryGetValue(retryMessage.Guid, out retryTimestamps) ||
                            retryTimestamps.HisTimestamps != null)
                            return;

                    var attempt = retryTimestamps.Attempts + 1;
                    if (attempt == _maxRetriesCount)
                    {
                        HaltRetry(retryMessage.Guid);
                        return;
                    }

                    retryTimestamps.Attempts = attempt;
                    _executor.AddMessage(new TimerRetryGossipMessage(retryMessage.Guid, _retryDelay, DateTimeOffset.Now,
                        _timerMessageId));
                    _incomingGossips.Add((retryMessage.Guid, null));
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

        private void Gossip()
        {
            try
            {
                while (true)
                {
                    var (guid, gossipMessage) = _incomingGossips.Take();
                    
                    (ZMI, IList<ValueContact>) zmiPair;
                    
                    lock (_zmis)
                        if (!_zmis.TryGetValue(guid, out zmiPair))
                        {
                            Logger.LogError($"Could not find zmi for guid {guid}, aborting gossiping then");
                            continue;
                        }

                    var (zmi, fallbacks) = zmiPair;

                    switch (gossipMessage)
                    {
                        case null:
                            bool infoExists;
                            TimestampsInfo timestampsInfo;
                            
                            lock (_timestamps)
                                infoExists = _timestamps.TryGetValue(guid, out timestampsInfo);
                            
                            var timestampsToSave = infoExists && timestampsInfo.HisTimestamps != null
                                ? RespondTimestamps(guid, timestampsInfo, zmi)
                                : SendInitialTimestamps(guid, zmi, fallbacks, infoExists ? timestampsInfo.Attempts : 1);
                            
                            lock (_timestamps)
                                _timestamps[guid] = timestampsToSave;
                            break;
                        case GossipAttributesMessage gossipAttributesMessage:
                            ProcessAttributeMessage(guid, gossipAttributesMessage, zmi);
                            break;
                        case GossipTimestampAskMessage _:
                            Logger.LogError("Should not get here, something wrong with logic previously");
                            break;
                        case GossipTimestampResponseMessage gossipTimestampResponseMessage:
                            ProcessTimestampResponse(guid, gossipTimestampResponseMessage, zmi);
                            break;
                    }
                }
            }
            catch (ThreadInterruptedException) {}
            catch (Exception e) { Logger.LogException(e); }
        }

        private TimestampsInfo SendInitialTimestamps(Guid guid, ZMI zmi, IList<ValueContact> fallbacks, int attempt)
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

            return new TimestampsInfo(contact, level, myTimestamps, null, attempt);
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

        private void ProcessTimestampResponse(Guid guid, GossipTimestampResponseMessage timestampResponseMessage, ZMI zmi)
        {
            TimestampsInfo timestampsInfo;
            
            lock (_timestamps)
                if (!_timestamps.TryGetValue(guid, out timestampsInfo))
                {
                    Logger.LogError(
                        $"Could not find timestamps when processing GossipTimestampResponseMessage. Aborting gossiping.");
                    return;
                }

            timestampsInfo.HisTimestamps = timestampResponseMessage.Timestamps;
            PrepareAndSendAttributes(guid, timestampsInfo, zmi, false);
        }

        private void ProcessAttributeMessage(Guid guid, GossipAttributesMessage attributesMessage, ZMI zmi)
        {
            TimestampsInfo timestampsInfo;
            
            lock (_timestamps)
                if (!_timestamps.TryGetValue(guid, out timestampsInfo))
                {
                    Logger.LogError(
                        $"Could not find timestamps when processing GossipTimestampResponseMessage. Aborting gossiping.");
                    return;
                }

            if (!attributesMessage.IsResponse)
                PrepareAndSendAttributes(guid, timestampsInfo, zmi, true);

            lock (_timestamps)
                _timestamps.Remove(guid);
            lock (_zmis)
                _zmis.Remove(guid);
            
            _executor.AddMessage(new ZMIProcessGossipedMessage(GetType(), attributesMessage.Attributes));
        }

        private void PrepareAndSendAttributes(Guid guid, TimestampsInfo timestampsInfo, ZMI zmi, bool isResponse)
        {
            var (myInterestingAttributes, hisInterestingZmis) =
                ExtractInterestingAttributes(zmi, timestampsInfo.MyTimestamps, timestampsInfo.HisTimestamps);
            
            var gossipMsg = new GossipAttributesMessage(guid, myInterestingAttributes, isResponse);
            var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule), gossipMsg,
                timestampsInfo.Contact.Address, timestampsInfo.Contact.Port);
            _executor.AddMessage(newMsg);
        }

        // TODO: Remove hisZmis
        private static (List<(PathName, AttributesMap)> myAttributes, IList<PathName> hisZmis)
            ExtractInterestingAttributes(ZMI zmi, IEnumerable<Timestamps> myTimestamps,
                IEnumerable<Timestamps> hisTimestamps)
        {
            var mySortedTimestamps = myTimestamps.ToList();
            mySortedTimestamps.Sort();
            var hisSortedTimestamps = hisTimestamps.ToList();
            hisSortedTimestamps.Sort();

            var i = 0;
            var j = 0;

            var myFather = zmi.GetFather();

            var myAttributes = new List<(PathName, AttributesMap)>();
            var hisZmis = new List<PathName>();

            void ExtractAndSaveMyAttr()
            {
                var pathName = mySortedTimestamps[i].PathName;
                if (!myFather.TrySearch(pathName.ToString(), out var foundZmi))
                {
                    Logger.LogWarning($"Couldn't find zmi for {pathName}");
                    return;
                }
                myAttributes.Add((pathName, foundZmi.Attributes.Clone() as AttributesMap));
            }

            while (i < mySortedTimestamps.Count && j < hisSortedTimestamps.Count)
            {
                if (mySortedTimestamps[i].CompareTo(hisSortedTimestamps[j]) < 0)
                {
                    ExtractAndSaveMyAttr();
                    i++;
                }
                else if (mySortedTimestamps[i].CompareTo(hisSortedTimestamps[j]) > 0)
                {
                    hisZmis.Add(hisSortedTimestamps[j].PathName);
                    j++;
                }
                else
                {
                    var timestampComp = mySortedTimestamps[i].CompareTimestamps(hisSortedTimestamps[j]);
                    if (timestampComp < 0)
                    {
                        hisZmis.Add(hisSortedTimestamps[j].PathName);
                    }
                    else if (timestampComp > 0)
                    {
                        ExtractAndSaveMyAttr();
                    }
                    
                    i++;
                    j++;
                }
            }

            while (i < mySortedTimestamps.Count)
            {
                ExtractAndSaveMyAttr();
                i++;
            }

            while (j < hisSortedTimestamps.Count)
            {
                hisZmis.Add(hisSortedTimestamps[j].PathName);
                j++;
            }

            return (myAttributes, hisZmis);
        }

        public void Dispose()
        {
            _gossipThread?.Interrupt();
            _incomingGossips?.Dispose();
        }
    }
}