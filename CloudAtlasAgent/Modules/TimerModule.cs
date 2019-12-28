using System;
using System.Collections.Concurrent;
using System.Threading;
using CloudAtlasAgent.Modules.Messages;
using Shared.Logger;

namespace CloudAtlasAgent.Modules
{
    public sealed class TimerModule : IModule
    {
        public bool Equals(IModule other) => other is TimerModule;

        private readonly BlockingCollection<TimerCallback> _priorityQueue =
            new BlockingCollection<TimerCallback>(new BlockingPriorityQueue<TimerCallback>());
        private readonly Thread _sleeperThread;

        public TimerModule()
        {
            var sleeper = new Sleeper(_priorityQueue);
            _sleeperThread = new Thread(sleeper.Run);
            _sleeperThread.Start();
        }

        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case TimerAddCallbackMessage timerAddCallbackMessage:
                    _priorityQueue.TryAdd(new TimerCallback(
                        DateTimeOffset.Now.Add(timerAddCallbackMessage.TimeFrom - DateTimeOffset.Now +
                                               TimeSpan.FromSeconds(timerAddCallbackMessage.Delay)),
                        timerAddCallbackMessage.Callback));
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

        // TODO: Better the implementation, this to be sure
        private sealed class TimerCallback : IComparable, IComparable<TimerCallback>, IEquatable<TimerCallback>
        {
            public DateTimeOffset Delay { get; }
            public Action Callback { get; }

            public TimerCallback(DateTimeOffset delay, Action callback)
            {
                Delay = delay;
                Callback = callback;
            }

            public int CompareTo(object? obj)
            {
                if (obj != null && !(obj is TimerCallback))
                    throw new ArgumentException("Object must be of type Piano.");

                return CompareTo(obj as TimerCallback);
            }
            
            public int CompareTo(TimerCallback other) => other == null ? 1 : Delay.CompareTo(other.Delay);

            public bool Equals(TimerCallback other) => other != null && ReferenceEquals(this, other);
        }

        private sealed class Sleeper
        {
            private readonly BlockingCollection<TimerCallback> _priorityQueue;
            public Sleeper(BlockingCollection<TimerCallback> priorityQueue) => _priorityQueue = priorityQueue;

            public void Run()
            {
                try
                {
                    while (true)
                    {
                        Logger.Log("NO ELO Z SLEEPERA");
                        var callback = _priorityQueue.Take();
                        Logger.Log($"Took {callback} out of priorityQueue");
                        var toSleep = callback.Delay - DateTimeOffset.Now;
                        if (toSleep.TotalMilliseconds > 0)
                            Thread.Sleep(toSleep);
                        Logger.Log("Sleepy sleeped");
                        callback.Callback();
                    }
                }
                catch (ObjectDisposedException) {}
                catch (OperationCanceledException) {}
                catch (ThreadInterruptedException) { Logger.LogError("Interrupted"); }
                catch (Exception e) { Logger.LogError(e.Message); }
            }
        }
    }
}