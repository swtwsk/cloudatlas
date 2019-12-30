using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CloudAtlasAgent.Modules.GossipStrategies;
using CloudAtlasAgent.Modules.Messages;
using Shared;
using Shared.Logger;
using Shared.Model;

namespace CloudAtlasAgent.Modules
{
    public class GossipModule : IModule
    {
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
            _executor.AddMessage(new GossipStartMessage(GetType(), GetType()));
        }

        private readonly int _gossipTimer;
        private int _timerMessageId = 0;

        private GossipModule _voidInstance;
        public IModule VoidInstance => _voidInstance ??= new GossipModule();

        private readonly Thread _gossipThread;

        private readonly IExecutor _executor;
        private IGossipStrategy _gossipStrategy;

        private readonly IDictionary<IMessage, (GossipInnerMessage gossip, bool isResponse)> _waitingForZMI =
            new Dictionary<IMessage, (GossipInnerMessage, bool)>();

        private readonly BlockingCollection<(GossipInnerMessage, bool, IList<ValueContact>, ZMI)> _forThread =
            new BlockingCollection<(GossipInnerMessage, bool, IList<ValueContact>, ZMI)>();
        
        private BlockingCollection<GossipInnerMessage> _toRespond = new BlockingCollection<GossipInnerMessage>();
        private BlockingCollection<GossipInnerMessage> _responses = new BlockingCollection<GossipInnerMessage>();

        public bool Equals(IModule other) => other is GossipModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as GossipModule);
        public override int GetHashCode() => "Gossip".GetHashCode();

        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case ZMIResponseMessage responseMessage:
                    if (!_waitingForZMI.TryGetValue(responseMessage.Request, out var pair))
                    {
                        Logger.LogError("Could not find request for ZMIResponse");
                        return;
                    }
                    _forThread.Add((pair.gossip, pair.isResponse, responseMessage.FallbackContacts, 
                        responseMessage.Zmi));
                    break;
                case GossipAskMessage gossipAskMessage:
                    var askMessage1 = new ZMIAskMessage(GetType(), typeof(ZMIModule));
                    _waitingForZMI.Add(askMessage1, (gossipAskMessage.GossipMessage, false));
                    _executor.AddMessage(askMessage1);
                    break;
                case GossipResponseMessage gossipResponseMessage:
                    var askMessage2 = new ZMIAskMessage(GetType(), typeof(ZMIModule));
                    _waitingForZMI.Add(askMessage2, (gossipResponseMessage.GossipMessage, true));
                    _executor.AddMessage(askMessage2);
                    break;
                case GossipStartMessage _:
                    var askMessage3 = new ZMIAskMessage(GetType(), typeof(ZMIModule));
                    _waitingForZMI.Add(askMessage3, default);
                    _executor.AddMessage(askMessage3);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        private static GossipInnerMessage PrepareGossipMessage(ZMI zmi, int level)
        {
            var toSend = zmi.AggregateAttributesAbove(level);
            if (!zmi.Attributes.TryGetValue("contacts", out var attrMyContacts) || attrMyContacts.IsNull ||
                !attrMyContacts.AttributeType.IsCompatible(
                    new AttributeTypeCollection(PrimaryType.Set, AttributeTypePrimitive.Contact)) ||
                ((ValueSet) attrMyContacts).Count != 1)
            {
                Logger.LogError("Could not find proper contacts set");
            }

            var myContact = (ValueContact) ((ValueSet) attrMyContacts).First();

            return new GossipInnerMessage(DateTimeOffset.Now, toSend, level, myContact);
        }

        private void Gossip()
        {
            try
            {
                while (true)
                {
                    var (gossipMessage, isResponse, fallbacks, zmi) = _forThread.Take();

                    if (gossipMessage != null)
                    {
                        ProcessGossip(gossipMessage, isResponse, zmi);
                        continue;
                    }
                    
                    if (!_gossipStrategy.TryGetContact(zmi, out var contact, out var level))
                    {
                        Logger.Log("RandomOrDefault to do");
                        contact = fallbacks.RandomOrDefault();
                        if (contact == null)
                        {
                            Logger.Log("No contacts at all");
                            // TODO: Multicast here
                            continue;
                        }
                    }
                    
                    //Logger.Log($"Contact = {contact}");
                    
                    var innerMessage = PrepareGossipMessage(zmi, level);
                    var gossipAskMsg = new GossipAskMessage(GetType(), typeof(GossipModule), innerMessage);

                    var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule),
                        gossipAskMsg, contact.Address, contact.Port);
                    _executor.AddMessage(newMsg);
                }
            }
            catch (ThreadInterruptedException) {}
            catch (Exception e) { Logger.LogException(e); }
        }

        private void ProcessGossip(GossipInnerMessage gossip, bool isResponse, ZMI zmi)
        {
            // TODO:
            Logger.Log($"Acquired gossip sent at {gossip.TimeStamp} from {gossip.Contact}, isResponse = {isResponse}");

            if (!isResponse)
            {
                var innerMessage = PrepareGossipMessage(zmi, gossip.Level);
                var gossipAskMsg = new GossipResponseMessage(GetType(), typeof(GossipModule), innerMessage);
                
                var contact = gossip.Contact;
                var newMsg = new CommunicationSendMessage(GetType(), typeof(CommunicationModule),
                    gossipAskMsg, contact.Address, contact.Port);
                _executor.AddMessage(newMsg);
            }
        }

        public void Dispose()
        {
            _gossipThread?.Interrupt();
            _forThread?.Dispose();
            _toRespond?.Dispose();
            _responses?.Dispose();
        }
    }
}