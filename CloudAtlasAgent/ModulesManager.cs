using System;
using System.Net;
using CloudAtlasAgent.Modules;
using CloudAtlasAgent.Modules.GossipStrategies;
using Grpc.Core;
using Shared.Logger;
using Shared.Model;

namespace CloudAtlasAgent
{
    public class ModulesManager : IDisposable
    {
        private readonly ExecutorRegistry _registry;
        private readonly Executor _executor;

        private TimerModule _timer;
        private CommunicationModule _communication;
        private ZMIModule _zmi;
        private RMIModule _rmi;
        private GossipModule _gossip;
        
        public ModulesManager(int maxPacketSize, string receiverHost, int receiverPort, int receiverTimeout,
            string rmiHost, int rmiPort, int queriesRecomputeTimer,
            int gossipTimer, int retryDelay, int maxRetriesCount, ZMI zmi)
        {
            Logger.Log("Creating modules...");
            _registry = new ExecutorRegistry();
            _executor = new Executor(_registry);
            
            void AddModule(IModule module)
            {
                if (_executor.TryAddModule(module))
                    return;
                Logger.LogError($"Could not add {module.GetType().Name}");
                throw new ApplicationException(
                    $"Could not add {module.GetType().Name}, which violates the application");
            }
            
            AddModule(_timer = new TimerModule(_executor));
            AddModule(_communication = new CommunicationModule(_executor, maxPacketSize, IPAddress.Parse(receiverHost), receiverPort, receiverTimeout));
            AddModule(_zmi = new ZMIModule(zmi, queriesRecomputeTimer, _executor));
            AddModule(_rmi = new RMIModule(_executor, new ServerPort(rmiHost, rmiPort, ServerCredentials.Insecure)));
            AddModule(_gossip = new GossipModule(_executor, gossipTimer, retryDelay, maxRetriesCount, new RoundRobinGossipStrategy()));
        }

        public void Dispose()
        {
            _registry.Dispose();
        }
    }
}
