﻿using System;
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
        private readonly HashSet<IModule> _modules = new HashSet<IModule>();
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
            if (_modules.Contains(module))
            {
                Logger.LogError($"Executor already has module {module}");
                return false;
            }
            if (_registry.ModuleExists(module))
            {
                Logger.LogError($"Other executor already has module {module}");
                return false;
            }
            _modules.Add(module);
            _registry.AddModule(module, this);
            return true;
        }

        private void HandleMessage()
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

            // switch (message)
            // {
            //     case TimerAddCallbackMessage _:
            //     case TimerRemoveCallbackMessage _:
            //         if (!_modules.TryGetValue(message.Destination, out module))
            //         {
            //             if (_registry.TryGetExecutor(message.Destination, out var msgExecutor))
            //                 msgExecutor.HandleMessage(message);
            //             Logger.LogError($"Could not find handler for {message}!");
            //             return;
            //         }
            //         module.HandleMessage(message);
            //         break;
            //     case CommunicationSendMessage sendMessage:
            //         
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(message));
            // }
        }

        public void Dispose()
        {
            foreach (var m in _modules)
                m.Dispose();
            
            _executorThread?.Interrupt();
        }
    }
}