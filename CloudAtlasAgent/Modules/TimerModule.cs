using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CloudAtlasAgent.Modules.Messages;
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
        private readonly Thread _sleeperThread;

        public TimerModule()
        {
            var sleeper = new Sleeper(_priorityQueue, _set);
            _sleeperThread = new Thread(sleeper.Start);
            _sleeperThread.Start();
        }

        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case TimerAddCallbackMessage timerAddCallbackMessage:
                    _priorityQueue.TryAdd(new TimerCallback(timerAddCallbackMessage));
                    break;
                case TimerRemoveCallbackMessage timerRemoveCallbackMessage:
                    lock (_set)
                        _set.Add(new TimerCallback(new DateTimeOffset(), timerRemoveCallbackMessage.Source,
                            timerRemoveCallbackMessage.RequestId, null));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
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
            private IModule Sender { get; }
            private int RequestId { get; }

            public TimerCallback(TimerAddCallbackMessage message) : this(
                DateTimeOffset.Now.Add(message.TimeFrom - DateTimeOffset.Now + TimeSpan.FromSeconds(message.Delay)),
                message.Source,
                message.RequestId,
                message.Callback) {}

            public TimerCallback(DateTimeOffset delay, IModule sender, int requestId, Action callback)
            {
                Delay = delay;
                Sender = sender;
                RequestId = requestId;
                Callback = callback;
            }

            public int CompareTo(object? obj)
            {
                if (obj != null && !(obj is TimerCallback))
                    throw new ArgumentException("Object must be of type Piano.");

                return CompareTo(obj as TimerCallback);
            }
            
            public int CompareTo(TimerCallback other) => other == null ? 1 : Delay.CompareTo(other.Delay);

            public bool Equals(TimerCallback other) =>
                other != null && Sender.Equals(other.Sender) && RequestId == other.RequestId;

            public override bool Equals(object? obj) => obj != null && Equals(obj as TimerCallback);

            public override int GetHashCode() => Sender.GetHashCode() % (RequestId + 1);
        }

        private sealed class Sleeper
        {
            private readonly BlockingCollection<TimerCallback> _priorityQueue;
            private readonly ISet<TimerCallback> _set;

            public Sleeper(BlockingCollection<TimerCallback> priorityQueue, ISet<TimerCallback> set)
            {
                _priorityQueue = priorityQueue;
                _set = set;
            }

            public void Start()
            {
                try
                {
                    while (true)
                    {
                        var callback = _priorityQueue.Take();
                        Logger.Log($"Took {callback} out of priorityQueue");
                        var toSleep = callback.Delay - DateTimeOffset.Now;
                        if (toSleep.TotalMilliseconds > 0)
                            Thread.Sleep(toSleep);  // TODO: Now it does not work
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