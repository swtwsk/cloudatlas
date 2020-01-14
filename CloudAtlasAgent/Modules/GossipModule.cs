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
using Shared.RPC;

namespace CloudAtlasAgent.Modules
{
    using QueryTuple = ValueTuple<string, long, SignedQuery>;
    
    public class GossipModule : IModule
    {
        public GossipModule(IExecutor executor, int gossipTimer, int retryDelay, int maxRetriesCount,
            IGossipStrategy gossipStrategy = null)
        {
            _executor = executor;
            _gossipStrategy = gossipStrategy ?? new RandomGossipStrategy();
            
            _gossipThread = new Thread(Gossip);
            _gossipThread.Start();
            
            _readerThread = new Thread(ReadMessages);
            _readerThread.Start();

            _gossipTimer = gossipTimer;
            _retryDelay = retryDelay;
            _maxRetriesCount = maxRetriesCount;

            lock (_timerMessageIdLock)
                _executor.AddMessage(new TimerAddCallbackMessage(GetType(), _timerMessageId++, _gossipTimer,
                    DateTimeOffset.Now, AddGossipTimer));
        }

        private void AddGossipTimer()
        {
            lock (_timerMessageIdLock)
                _executor.AddMessage(new TimerAddCallbackMessage(GetType(), _timerMessageId++, _gossipTimer,
                    DateTimeOffset.Now, AddGossipTimer));
            _executor.AddMessage(new GossipStartMessage());
        }

        private readonly int _gossipTimer;
        private readonly int _retryDelay;
        private readonly int _maxRetriesCount;
        private int _timerMessageId = 0;
        private readonly object _timerMessageIdLock = new object();

        private readonly Thread _gossipThread;
        private readonly Thread _readerThread;

        private readonly IExecutor _executor;
        private readonly IGossipStrategy _gossipStrategy;

        private readonly IDictionary<Guid, (ZMI, IList<ValueContact>, IList<QueryTuple>)> _zmis =
            new Dictionary<Guid, (ZMI, IList<ValueContact>, IList<QueryTuple>)>();
        private readonly IDictionary<Guid, TimestampsInfo> _timestamps = new Dictionary<Guid, TimestampsInfo>();
        private readonly IDictionary<Guid, int> _retryIds = new Dictionary<Guid, int>();

        private readonly BlockingCollection<IMessage> _incomingMessages = new BlockingCollection<IMessage>();

        private readonly BlockingCollection<(Guid, GossipMessageBase)> _incomingGossips =
            new BlockingCollection<(Guid, GossipMessageBase)>();

        private class TimestampsInfo
        {
            public ValueContact Contact { get; }
            public int Level { get; }
            public IList<Timestamps> MyTimestamps { get; }
            public IList<Timestamps> HisTimestamps { get; set; }
            public IList<string> MyQueryNames { get; }
            public IList<string> HisQueryNames { get; set; }
            
            public ValueTime MySendTime { get; set; }
            public ValueTime HisReceiveTime { get; set; }
            public ValueTime HisSendTime { get; set; }
            public ValueTime MyReceiveTime { get; set; }
            
            public int Attempts { get; set; }

            public TimestampsInfo(ValueContact contact, int level, IList<Timestamps> myTimestamps,
                IList<Timestamps> hisTimestamps, IList<string> myQueryNames, IList<string> hisQueryNames,
                int attempts = 1)
            {
                Contact = contact;
                Level = level;
                MyTimestamps = myTimestamps;
                HisTimestamps = hisTimestamps;
                MyQueryNames = myQueryNames;
                HisQueryNames = hisQueryNames;
                Attempts = attempts;
            }
            
            public TimestampsInfo(ValueContact contact, int level, IList<Timestamps> myTimestamps,
                IList<Timestamps> hisTimestamps, IList<string> myQueryNames, IList<string> hisQueryNames,
                ValueTime hisSendTime, ValueTime myReceiveTime) 
                : this(contact, level, myTimestamps, hisTimestamps, myQueryNames, hisQueryNames)
            {
                HisSendTime = hisSendTime;
                MyReceiveTime = myReceiveTime;
            }

            public TimestampsInfo(GossipTimestampAsk askMessage)
                : this(askMessage.Contact, askMessage.Level, null, askMessage.Timestamps, null, askMessage.QueryNames)
            {
                HisSendTime = askMessage.SendTimestamp;
                MyReceiveTime = askMessage.ReceiveTimestamp;
            }

            public void Deconstruct(out ValueContact contact, out int level, out IList<Timestamps> myTimestamps,
                out IList<Timestamps> hisTimestamps, out IList<string> myQueryNames, out IList<string> hisQueryNames)
            {
                contact = Contact;
                level = Level;
                myTimestamps = MyTimestamps;
                hisTimestamps = HisTimestamps;
                myQueryNames = MyQueryNames;
                hisQueryNames = HisQueryNames;
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

        private void ReadMessages()
        {
            try
            {
                while (true)
                {
                    var message = _incomingMessages.Take();
                    switch (message)
                    {
                        case ZMIResponseMessage responseMessage:
                            lock (_zmis)
                                _zmis.TryAdd(responseMessage.RequestGuid,
                                    (responseMessage.Zmi, responseMessage.FallbackContacts, responseMessage.Queries));
                            _incomingGossips.Add((responseMessage.RequestGuid, null));
                            break;
                        case GossipTimestampAsk gossipAskMessage:
                            lock (_timestamps)
                                _timestamps[gossipAskMessage.Guid] = new TimestampsInfo(gossipAskMessage);
                            _executor.AddMessage(new ZMIAskMessage(GetType(), gossipAskMessage.Guid));
                            break;
                        case GossipTimestampResponseMessage gossipResponseMessage:
                            var guid = gossipResponseMessage.Guid;

                            lock (_timestamps)
                            {
                                if (!_timestamps.TryGetValue(guid, out var timestamps))
                                {
                                    Logger.LogError($"Could not find timestamp infos for guid {guid}");
                                    continue;
                                }
                                
                                timestamps.HisTimestamps = gossipResponseMessage.Timestamps;
                                timestamps.HisQueryNames = gossipResponseMessage.QueryNames;
                                timestamps.MySendTime = gossipResponseMessage.RequestSendTimestamp;
                                timestamps.HisReceiveTime = gossipResponseMessage.RequestReceiveTimestamp;
                                timestamps.HisSendTime = gossipResponseMessage.ResponseSendTimestamp;
                                timestamps.MyReceiveTime = gossipResponseMessage.ResponseReceiveTimestamp;
                            }

                            HaltRetry(gossipResponseMessage.Guid);
                            _incomingGossips.Add((gossipResponseMessage.Guid, gossipResponseMessage));
                            break;
                        case GossipAttributesMessage gossipAttributesMessage:
                            _incomingGossips.Add((gossipAttributesMessage.Guid, gossipAttributesMessage));
                            break;
                        case GossipStartMessage _:
                            var newGuid = Guid.NewGuid();
                            lock (_timestamps)
                                _timestamps[newGuid] = new TimestampsInfo(null, -1, null, null, null, null);
                            _executor.AddMessage(new ZMIAskMessage(GetType(), newGuid));

                            int retryId;
                            lock (_timerMessageIdLock)
                            {
                                retryId = _timerMessageId++;
                            }

                            _executor.AddMessage(new TimerRetryGossipMessage(newGuid, _retryDelay, DateTimeOffset.Now,
                                retryId));
                            _retryIds.Add(newGuid, retryId);
                            break;
                        case GossipRetryMessage retryMessage:
                            lock (_timestamps)
                            {
                                if (!_timestamps.TryGetValue(retryMessage.Guid, out var retryTimestamps) ||
                                    retryTimestamps == null ||
                                    retryTimestamps.HisTimestamps != null) // TODO: this is due to no-multicast bug
                                    continue;

                                var attempt = retryTimestamps.Attempts + 1;
                                if (attempt >= _maxRetriesCount)
                                {
                                    HaltRetry(retryMessage.Guid);
                                    continue;
                                }

                                retryTimestamps.Attempts = attempt;
                            }

                            lock (_timerMessageIdLock)
                                _executor.AddMessage(new TimerRetryGossipMessage(retryMessage.Guid, _retryDelay,
                                    DateTimeOffset.Now, _timerMessageId++));
                            _incomingGossips.Add((retryMessage.Guid, null));
                            break;
                        default:
                            Logger.LogException(new ArgumentOutOfRangeException());
                            break;
                    }
                }
            }
            catch (ThreadInterruptedException) {}
            catch (ObjectDisposedException oe) { Logger.LogException(oe); }
            catch (Exception e) { Logger.LogException(e); }
        }

        public void HandleMessage(IMessage message)
        {
            _incomingMessages.Add(message);
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
                    
                    (ZMI, IList<ValueContact>, IList<QueryTuple>) zmiTuple;
                    
                    lock (_zmis)
                        if (!_zmis.TryGetValue(guid, out zmiTuple))
                        {
                            Logger.LogError($"Could not find zmi for guid {guid}, aborting gossiping then");
                            continue;
                        }

                    var (zmi, fallbacks, queries) = zmiTuple;

                    switch (gossipMessage)
                    {
                        case null:
                            lock (_timestamps)
                            {
                                var infoExists = _timestamps.TryGetValue(guid, out var timestampsInfo);
                                var queryNames = queries.Select(tuple => tuple.Item1).ToList();

                                var timestampsToSave = infoExists && timestampsInfo.HisTimestamps != null
                                    ? RespondTimestamps(guid, timestampsInfo, zmi, queryNames)
                                    : SendInitialTimestamps(guid, zmi, fallbacks, queryNames, infoExists ? timestampsInfo.Attempts : 1);

                                if (timestampsToSave != null)
                                    _timestamps[guid] = timestampsToSave;
                            }

                            break;
                        case GossipAttributesMessage gossipAttributesMessage:
                            ProcessAttributeMessage(guid, gossipAttributesMessage, zmi);
                            break;
                        case GossipTimestampAsk _:
                            Logger.LogError("Should not get here, something wrong with logic previously");
                            break;
                        case GossipTimestampResponseMessage gossipTimestampResponseMessage:
                            ProcessTimestampResponse(guid, gossipTimestampResponseMessage, zmi);
                            break;
                    }
                }
            }
            catch (ThreadInterruptedException) {}
            catch (ObjectDisposedException) {}
            catch (Exception e) { Logger.LogException(e); }
        }

        private TimestampsInfo SendInitialTimestamps(Guid guid, ZMI zmi, IList<ValueContact> fallbacks,
            IList<string> queryNames, int attempt)
        {
            if (!_gossipStrategy.TryGetContact(zmi, out var contact, out var level))
            {
                Logger.Log("RandomOrDefault to do");
                contact = fallbacks.RandomOrDefault();
                // TODO: level here
                if (contact == null)
                {
                    Logger.LogWarning("No contacts at all");
                    // TODO: Multicast here
                    // return null
                    return new TimestampsInfo(null, level, null, null, null, null, _maxRetriesCount);
                }
            }
            else
            {
                Logger.Log($"Found element at level {level}");
            }

            Logger.Log($"Sending initial timestamps to {contact}");
            var (myTimestamps, myContact) = PrepareGossipTimestampsMessage(zmi, level);
            var gossipAskMsg = new GossipTimestampAsk(guid, myTimestamps, queryNames, level, myContact);
            
            var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule),
                gossipAskMsg, contact.Address, contact.Port);
            _executor.AddMessage(newMsg);

            return new TimestampsInfo(contact, level, myTimestamps, null, queryNames, null, attempt);
        }

        private TimestampsInfo RespondTimestamps(Guid guid, TimestampsInfo message, ZMI zmi, IList<string> queries)
        {
            var (contact, level, _, hisTimestamps, _, hisQueryNames) = message;
            
            Logger.Log($"Acquired timestamps from {contact}");

            var (myTimestamps, _) = PrepareGossipTimestampsMessage(zmi, level);
            var gossipResponseMessage = new GossipTimestampResponseMessage(guid, myTimestamps, queries,
                message.HisSendTime, message.MyReceiveTime);

            var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule),
                gossipResponseMessage, contact.Address, contact.Port);
            _executor.AddMessage(newMsg);

            return new TimestampsInfo(contact, level, myTimestamps, hisTimestamps, queries, hisQueryNames,
                message.HisSendTime, message.MyReceiveTime);
        }

        private void ProcessTimestampResponse(Guid guid, GossipTimestampResponseMessage timestampResponseMessage, ZMI zmi)
        {
            lock (_timestamps)
            {
                if (!_timestamps.TryGetValue(guid, out var timestampsInfo))
                {
                    Logger.LogError(
                        $"Could not find timestamps when processing GossipTimestampResponseMessage. Aborting gossiping.");
                    return;
                }

                var myDelay = timestampResponseMessage.ResponseReceiveTimestamp.Subtract(timestampResponseMessage.RequestSendTimestamp);
                var hisDelay = timestampResponseMessage.ResponseSendTimestamp.Subtract(timestampResponseMessage.RequestReceiveTimestamp);
                var rtd = (ValueDuration) myDelay.Subtract(hisDelay);
                var delay = (ValueDuration) timestampResponseMessage.ResponseSendTimestamp
                    .Add(rtd.Divide(new ValueInt(2))).Subtract(timestampResponseMessage.ResponseReceiveTimestamp);
                PrepareAndSendAttributes(guid, timestampsInfo, zmi, delay, false);
            }
        }

        private void ProcessAttributeMessage(Guid guid, GossipAttributesMessage attributesMessage, ZMI zmi)
        {
            lock (_timestamps)
            {
                if (!_timestamps.TryGetValue(guid, out var timestampsInfo))
                {
                    Logger.LogError(
                        $"Could not find timestamps when processing GossipTimestampResponseMessage. Aborting gossiping.");
                    return;
                }

                if (!attributesMessage.IsResponse)
                    PrepareAndSendAttributes(guid, timestampsInfo, zmi, attributesMessage.Delay, true);

                _timestamps.Remove(guid);
            }

            lock (_zmis)
                _zmis.Remove(guid);

            _executor.AddMessage(new ZMIProcessGossipedMessage(GetType(), attributesMessage.Attributes,
                attributesMessage.Queries, attributesMessage.Delay));
        }

        private void PrepareAndSendAttributes(Guid guid, TimestampsInfo timestampsInfo, ZMI zmi, ValueDuration delay, 
            bool isResponse)
        {
            (ZMI, IList<ValueContact>, IList<QueryTuple>) zmiInfo;
            
            lock (_zmis)
            {
                if (!_zmis.TryGetValue(guid, out zmiInfo))
                {
                    Logger.LogError(
                        $"Could not find zmiInfo when processing GossipTimestampResponseMessage. Aborting gossiping.");
                    return;
                }
            }
            
            var myInterestingAttributes =
                ExtractInterestingAttributes(zmi, timestampsInfo.MyTimestamps, timestampsInfo.HisTimestamps, delay);
            var myInterestingQueries = ExtractInterestingQueries(timestampsInfo.Level, zmiInfo.Item3, timestampsInfo.HisQueryNames);

            var gossipMsg = new GossipAttributesMessage(guid, myInterestingAttributes, (ValueDuration) delay.Negate(),
                myInterestingQueries, isResponse);
            var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule), gossipMsg,
                timestampsInfo.Contact.Address, timestampsInfo.Contact.Port);
            _executor.AddMessage(newMsg);
        }

        private static List<(PathName, AttributesMap)> ExtractInterestingAttributes(ZMI zmi, 
            IEnumerable<Timestamps> myTimestamps, IEnumerable<Timestamps> hisTimestamps, ValueDuration delay)
        {
            var mySortedTimestamps = myTimestamps.ToList();
            mySortedTimestamps.ForEach(ts => ts.ApplyFunc(time => (ValueTime) time.Add(delay)));
            mySortedTimestamps.Sort();
            
            var hisSortedTimestamps = hisTimestamps.ToList();
            hisSortedTimestamps.ForEach(ts => ts.ApplyFunc(time => (ValueTime) time.Add(delay)));
            hisSortedTimestamps.Sort();

            var i = 0;
            var j = 0;

            var myFather = zmi.GetFather();

            var myAttributes = new List<(PathName, AttributesMap)>();

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
                    j++;
                }
                else
                {
                    var timestampComp = mySortedTimestamps[i].CompareTimestamps(hisSortedTimestamps[j]);
                    if (timestampComp > 0)
                        ExtractAndSaveMyAttr();
                    
                    i++;
                    j++;
                }
            }

            while (i < mySortedTimestamps.Count)
            {
                ExtractAndSaveMyAttr();
                i++;
            }

            return myAttributes;
        }

        private static List<(long, SignedQuery)> ExtractInterestingQueries(long gossipLevel, IEnumerable<QueryTuple> myQueries, IEnumerable<string> hisQueries)
        {
            var valueTuples = myQueries.ToList();
            
            var myQueryNames = valueTuples.Select(tuple => tuple.Item1);
            var interestingQueryNames = myQueryNames.Except(hisQueries).ToHashSet();

            return valueTuples.Where(tuple => interestingQueryNames.Contains(tuple.Item1))
                .Select(tuple => (Math.Min(tuple.Item2, gossipLevel - 1), tuple.Item3)).ToList();
        }

        public void Dispose()
        {
            _gossipThread?.Interrupt();
            _incomingGossips?.Dispose();
            _readerThread?.Interrupt();
            _incomingMessages?.Dispose();
        }
    }
}