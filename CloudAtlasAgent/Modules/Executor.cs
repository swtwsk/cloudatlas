using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CloudAtlasAgent.Modules.Messages;
using Shared.Logger;

namespace CloudAtlasAgent.Modules
{
    public interface IExecutor
    {
        void AddMessage(IMessage message);
    }
    
    public class Executor : IDisposable, IExecutor
    {
        private readonly IDictionary<Type, IModule> _modules = new Dictionary<Type, IModule>();
        private readonly ExecutorRegistry _registry;
        
        private readonly BlockingCollection<IMessage> _messages = new BlockingCollection<IMessage>();

        private readonly Thread _executorThread;

        public Executor(ExecutorRegistry registry)
        {
            _registry = registry;
            _registry.AddExecutor(this);
            _executorThread = new Thread(HandleMessage);
            _executorThread.Start();
        }
        
        public void AddMessage(IMessage message) => _messages.Add(message);

        public bool TryAddModule(IModule module)
        {
            if (_modules.ContainsKey(module.GetType()))
            {
                Logger.LogError($"Executor already has module {module}");
                return false;
            }
            if (_registry.ModuleExists(module))
            {
                Logger.LogError($"Other executor already has module {module}");
                return false;
            }
            _modules.Add(module.GetType(), module);
            _registry.AddModule(module, this);
            return true;
        }

        private void HandleMessage()
        {
            try
            {
                while (true)
                {
                    var message = _messages.Take();
                    if (!_modules.TryGetValue(message.Destination, out var module))
                    {
                        if (_registry.TryGetExecutor(message.Destination, out var msgExecutor))
                            msgExecutor.AddMessage(message);
                        Logger.LogError($"Could not find handler for {message}!");
                        throw new ArgumentOutOfRangeException(nameof(message));
                    }

                    module.HandleMessage(message);
                }
            }
            catch (ThreadInterruptedException) { Logger.LogWarning("Executor thread interrupted"); }
            catch (ObjectDisposedException e) { Logger.LogException(e); }
            catch (Exception e) { Logger.LogException(e); }
        }


        public void Dispose()
        {
            _messages?.Dispose();
            _executorThread?.Interrupt();
            
            foreach (var m in _modules.Values)
                m.Dispose();
        }
    }
}