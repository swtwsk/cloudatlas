﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CloudAtlasAgent.Modules.Messages;
using CloudAtlasAgent.Modules.Messages.GossipMessages;
using Shared.Logger;

namespace CloudAtlasAgent.Modules
{
    public sealed class TimerModule : IModule
    {
        public bool Equals(IModule other) => other is TimerModule;

        public override bool Equals(object? obj) => obj != null && Equals(obj as TimerModule);

        public override int GetHashCode() => "Timer".GetHashCode();

        private readonly BlockingCollection<TimerCallback> _priorityQueue =
            new BlockingCollection<TimerCallback>(new BlockingPriorityQueue<TimerCallback>());
        private readonly ISet<TimerCallback> _set = new HashSet<TimerCallback>();
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly Thread _sleeperThread;

        private readonly IExecutor _executor;

        public TimerModule(IExecutor executor)
        {
            _executor = executor;
            var sleeper = new Sleeper(_priorityQueue, _set, _resetEvent);
            _sleeperThread = new Thread(sleeper.Start);
            _sleeperThread.Start();
        }
        
        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case TimerAddCallbackMessage timerAddCallbackMessage:
                    AddToQueue(new TimerCallback(timerAddCallbackMessage));
                    break;
                case TimerRemoveCallbackMessage timerRemoveCallbackMessage:
                    lock (_set)
                        _set.Add(new TimerCallback(timerRemoveCallbackMessage.Source,
                            timerRemoveCallbackMessage.RequestId, null));
                    break;
                case TimerRetryGossipMessage retryGossipMessage:
                    AddToQueue(new TimerCallback(
                        TimerCallback.ComputeDelayedTimestamp(retryGossipMessage.TimeStamp, retryGossipMessage.Delay),
                        typeof(GossipModule),
                        retryGossipMessage.RequestId,
                        () => _executor.AddMessage(
                            new GossipRetryMessage(typeof(GossipModule), retryGossipMessage.Guid))));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        private void AddToQueue(TimerCallback timerCallback)
        {
            while (!_priorityQueue.TryAdd(timerCallback))
                Logger.LogError("Could not add TimerCallback to priorityQueue!");
            _resetEvent.Set();
        }
        
        public void Dispose()
        {
            _sleeperThread.Interrupt();
            _priorityQueue?.Dispose();
        }

        private sealed class TimerCallback : IComparable, IComparable<TimerCallback>, IEquatable<TimerCallback>
        {
            public DateTimeOffset Delay { get; }
            public Action Callback { get; }
            private Type Sender { get; }
            private int RequestId { get; }

            public TimerCallback(TimerAddCallbackMessage message) : this(
                ComputeDelayedTimestamp(message.TimeFrom, message.Delay),
                message.Source,
                message.RequestId,
                message.Callback) {}

            public TimerCallback(Type sender, int requestId, Action callback) 
                : this(new DateTimeOffset(), sender, requestId, callback) {}

            public TimerCallback(DateTimeOffset delay, Type sender, int requestId, Action callback)
            {
                Delay = delay;
                Sender = sender;
                RequestId = requestId;
                Callback = callback;
            }

            public static DateTimeOffset ComputeDelayedTimestamp(DateTimeOffset timestamp, int delay) =>
                DateTimeOffset.Now.Add(timestamp - DateTimeOffset.Now + TimeSpan.FromSeconds(delay));

            public int CompareTo(object? obj)
            {
                if (obj != null && !(obj is TimerCallback))
                    throw new ArgumentException("Object must be of type Piano.");

                return CompareTo(obj as TimerCallback);
            }
            
            public int CompareTo(TimerCallback other) => other == null ? 1 : Delay.CompareTo(other.Delay);

            public bool Equals(TimerCallback other) =>
                other != null && Sender == other.Sender && RequestId == other.RequestId;

            public override bool Equals(object? obj) => obj != null && Equals(obj as TimerCallback);

            public override int GetHashCode() => Sender.GetHashCode() % (RequestId + 1);
        }

        private sealed class Sleeper
        {
            private readonly BlockingCollection<TimerCallback> _priorityQueue;
            private readonly ISet<TimerCallback> _set;
            private readonly AutoResetEvent _resetEvent;

            public Sleeper(BlockingCollection<TimerCallback> priorityQueue, ISet<TimerCallback> set, 
                AutoResetEvent resetEvent)
            {
                _priorityQueue = priorityQueue;
                _set = set;
                _resetEvent = resetEvent;
            }

            public void Start()
            {
                try
                {
                    while (true)
                    {
                        var callback = _priorityQueue.Take();
                        Logger.Log($"Took {callback.Callback.Method} out of priorityQueue");
                        
                        while (true)
                        {
                            var toSleep = callback.Delay - DateTimeOffset.Now;
                            if (toSleep.TotalMilliseconds <= 0)
                                break;
                            
                            Logger.Log($"About to sleep {toSleep.TotalSeconds} seconds");
                            if (_resetEvent.WaitOne(toSleep))
                            {
                                if (!_priorityQueue.TryTake(out var newCallback))
                                    continue;

                                if (newCallback.Delay >= callback.Delay)
                                {
                                    while (!_priorityQueue.TryAdd(newCallback)) {}
                                    continue;
                                }

                                while (!_priorityQueue.TryAdd(callback)) {}

                                callback = newCallback;
                            }
                            else
                            {
                                break;  // uninterrupted sleep
                            }
                        }

                        lock (_set)
                        {
                            if (_set.Remove(callback))
                                continue;
                        }
                        
                        callback.Callback();
                    }
                }
                catch (ObjectDisposedException) {}
                catch (OperationCanceledException) {}
                catch (ThreadInterruptedException) {}
                catch (Exception e) { Logger.LogException(e); }
            }
        }
    }
}