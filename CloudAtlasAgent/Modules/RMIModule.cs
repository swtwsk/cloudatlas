using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CloudAtlasAgent.Modules.Messages;
using CloudAtlasAgent.Modules.Messages.ZMIMessages;
using Grpc.Core;
using Shared;
using Shared.Logger;
using Shared.Model;
using Shared.RPC;

namespace CloudAtlasAgent.Modules
{
    public class RMIModule : IModule
    {
        public bool Equals(IModule other) => other is RMIModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as RMIModule);
        public override int GetHashCode() => "RMI".GetHashCode();

        private readonly IExecutor _executor;
        private readonly ZMIModule _zmiModule;
        
        private readonly object _dictLock = new object();
        private readonly IDictionary<IMessage, IMessage> _dictionary = new Dictionary<IMessage, IMessage>();

        private readonly Thread _serverThread;
        private readonly Grpc.Core.Server _server;
        
        public RMIModule(IExecutor executor, ZMIModule zmiModule, ServerPort serverPort)
        {
            _executor = executor;
            _zmiModule = zmiModule;
            _server = new Grpc.Core.Server
            {
                Ports = {serverPort},
                Services =
                {
                    ServerServiceDefinition.CreateBuilder()
                        .AddMethod(AgentMethods.GetZones, GetZones)
                        .AddMethod(AgentMethods.GetAttributes, GetAttributes)
                        .Build()
                }
            };
            _serverThread = new Thread(_server.Start);
            _serverThread.Start();
            Console.WriteLine($"{_server}");
            // _serverTask = RunServer(serverPort);
            // _serverTask.Start();
        }

        public void Dispose()
        {
            _serverThread?.Interrupt();
            _server?.ShutdownAsync();
            // _serverTask?.Dispose();
        }
        
        private async Task RunServer(ServerPort serverPort)
        {
            var server = new Grpc.Core.Server
            {
                Ports = {serverPort},
                Services =
                {
                    ServerServiceDefinition.CreateBuilder()
                        .AddMethod(AgentMethods.GetZones, GetZones)
                        .AddMethod(AgentMethods.GetAttributes, GetAttributes)
                        // .AddMethod(AgentMethods.GetQueries, GetQueries)
                        // .AddMethod(AgentMethods.InstallQuery, InstallQuery)
                        // .AddMethod(AgentMethods.UninstallQuery, UninstallQuery)
                        // .AddMethod(AgentMethods.SetAttribute, SetAttribute)
                        // .AddMethod(AgentMethods.SetContacts, SetContacts)
                        .Build()
                }
            };

            server.Start();
            Console.WriteLine($"Server started under [{serverPort.Host}:{serverPort.Port}]. Press Enter to stop it...");
            Console.ReadLine();  // TODO: Make it better XD

            await server.ShutdownAsync();
        }
        
        private Task<HashSet<string>> GetZones(Empty _, ServerCallContext ctx)
        {
            Logger.Log("GetZones");
            
            var requestMsg = new GetZonesRequestMessage(this, _zmiModule);
            _executor.AddMessage(requestMsg);

            IMessage responseMessage;

            lock (_dictLock)
            {
                while (!_dictionary.TryGetValue(requestMsg, out responseMessage))
                    Monitor.Wait(_dictLock);
            }

            if (!(responseMessage is GetZonesResponseMessage getZonesResponseMessage))
            {
                Logger.LogError("Return message is not a GetZonesResponseMessage");
                return Task.FromResult(new HashSet<string>());
            }
            
            return Task.FromResult(getZonesResponseMessage.Response);
        }

        private Task<AttributesMap> GetAttributes(string pathName, ServerCallContext ctx)
        {
            Logger.Log($"GetAttributes({pathName})");
            
            var requestMsg = new GetAttributesRequestMessage(this, _zmiModule, pathName);
            _executor.AddMessage(requestMsg);

            IMessage responseMessage;

            lock (_dictLock)
            {
                while (!_dictionary.TryGetValue(requestMsg, out responseMessage))
                    Monitor.Wait(_dictLock);
            }

            if (!(responseMessage is GetAttributesResponseMessage getAttributesResponseMessage))
            {
                Logger.LogError("Return message is not a GetZonesResponseMessage");
                return Task.FromResult(new AttributesMap());
            }
            
            return Task.FromResult(getAttributesResponseMessage.Response);
        }

        public void HandleMessage(IMessage message)
        {
            switch (message)
            {
                case IZMIResponseMessage<object> responseMessage:
                    lock (_dictLock)
                    {
                        _dictionary.Add(responseMessage.Request, responseMessage);
                        Monitor.PulseAll(_dictLock);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }
    }
}
