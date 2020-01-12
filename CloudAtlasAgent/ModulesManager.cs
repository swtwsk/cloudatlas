using System;
using System.Net;
using System.Security.Cryptography;
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

        private TimerModule _timer;
        private CommunicationModule _communication;
        private ZMIModule _zmi;
        private RMIModule _rmi;
        private GossipModule _gossip;
        
        public ModulesManager(int maxPacketSize, string receiverHost, int receiverPort, int receiverTimeout,
            string rpcHost, int rpcPort, int queriesRecomputeTimer, int purgeTimer, RSA rsa,
            IGossipStrategy gossipStrategy, int gossipTimer, int retryDelay, int maxRetriesCount, ZMI zmi)
        {
            Logger.Log("Creating modules...");
            _registry = new ExecutorRegistry();
            var executor = new Executor(_registry);
            
            void AddModule(IModule module)
            {
                if (executor.TryAddModule(module))
                    return;
                Logger.LogError($"Could not add {module.GetType().Name}");
                throw new ApplicationException(
                    $"Could not add {module.GetType().Name}, which violates the application");
            }
            
            Console.WriteLine($"Agent started on {receiverHost}:{receiverPort}\nRPC started on {rpcHost}:{rpcPort}");
            
            AddModule(_timer = new TimerModule(executor));
            AddModule(_communication = new CommunicationModule(executor, maxPacketSize, IPAddress.Parse(receiverHost), receiverPort, receiverTimeout));
            AddModule(_zmi = new ZMIModule(zmi, rsa, queriesRecomputeTimer, purgeTimer, executor));
            AddModule(_rmi = new RMIModule(executor, new ServerPort(rpcHost, rpcPort, ServerCredentials.Insecure)));
            AddModule(_gossip = new GossipModule(executor, gossipTimer, retryDelay, maxRetriesCount, gossipStrategy));
        }

        public void Dispose()
        {
            _registry.Dispose();
        }
    }
}
