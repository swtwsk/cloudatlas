using System;
using System.Net;
using CloudAtlasAgent.Modules;
using CloudAtlasAgent.Modules.Messages;
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
            string rmiHost, int rmiPort, 
            int gossipTimer, ZMI zmi)
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
            
            AddModule(_timer = new TimerModule());
            AddModule(_communication = new CommunicationModule(_executor, maxPacketSize, IPAddress.Parse(receiverHost), receiverPort, receiverTimeout));
            AddModule(_zmi = new ZMIModule(zmi, _executor));
            AddModule(_rmi = new RMIModule(_executor, new ServerPort(rmiHost, rmiPort, ServerCredentials.Insecure)));
            AddModule(_gossip = new GossipModule(_executor, gossipTimer));
        }

        public void Start()
        {
            //TestModule();
        }

        public void Dispose()
        {
            _registry.Dispose();
        }

        private static void PrintTest()
        {
            Console.WriteLine($"TEST ME");
        }

        private void TestModule()
        {
            _executor.AddMessage(new TimerAddCallbackMessage(typeof(DummyModule), typeof(TimerModule), 0, 1, DateTimeOffset.Now, 
                PrintTest));

            _executor.AddMessage(new TimerAddCallbackMessage(typeof(DummyModule), typeof(TimerModule), 0, 8, DateTimeOffset.Now,
                () => Console.WriteLine("TEST ME 0")));
            _executor.AddMessage(new TimerAddCallbackMessage(typeof(DummyModule), typeof(TimerModule), 1, 8, DateTimeOffset.Now,
                () => Console.WriteLine("TEST ME 1")));
            _executor.AddMessage(new TimerAddCallbackMessage(typeof(DummyModule), typeof(TimerModule), 2, 1, DateTimeOffset.Now,
                () => Console.WriteLine("TEST ME 2")));
            _executor.AddMessage(new TimerAddCallbackMessage(typeof(DummyModule), typeof(TimerModule), 3, 4, DateTimeOffset.Now,
                () => Console.WriteLine("TEST ME 3")));
            _executor.AddMessage(new TimerAddCallbackMessage(typeof(DummyModule), typeof(TimerModule), 4, 2, DateTimeOffset.Now,
                () => Console.WriteLine("TEST ME 4")));
            _executor.AddMessage(new TimerAddCallbackMessage(typeof(DummyModule), typeof(TimerModule), 5, 4, DateTimeOffset.Now,
                () => Console.WriteLine("TEST ME 5")));
            _executor.AddMessage(new TimerRemoveCallbackMessage(typeof(DummyModule), typeof(TimerModule), 3));
        }
    }
}
