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
	// Should be named RPC Module (as in C# there's no RMIs) but named this way for consistence
    public class RMIModule : IModule
    {
        public bool Equals(IModule other) => other is RMIModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as RMIModule);
        public override int GetHashCode() => "RMI".GetHashCode();

        private readonly IExecutor _executor;
        
        private readonly object _dictLock = new object();
        private readonly IDictionary<IMessage, IMessage> _dictionary = new Dictionary<IMessage, IMessage>();

        private readonly Thread _serverThread;
        private readonly Grpc.Core.Server _server;
        
        public RMIModule(IExecutor executor, ServerPort serverPort)
        {
            _executor = executor;
            _server = new Grpc.Core.Server
            {
                Ports = {serverPort},
                Services =
                {
                    ServerServiceDefinition.CreateBuilder()
                        .AddMethod(AgentMethods.GetZones, GetZones)
                        .AddMethod(AgentMethods.GetAttributes, GetAttributes)
                        .AddMethod(AgentMethods.GetQueries, GetQueries)
                        .AddMethod(AgentMethods.InstallQuery, InstallQuery)
                        .AddMethod(AgentMethods.UninstallQuery, UninstallQuery)
                        .AddMethod(AgentMethods.SetAttribute, SetAttribute)
                        .AddMethod(AgentMethods.SetContacts, SetContacts)
                        .Build()
                }
            };
            _serverThread = new Thread(_server.Start);
            _serverThread.Start();
        }

        public void Dispose()
        {
            _serverThread?.Interrupt();
            _server?.ShutdownAsync();
        }

        private Task<HashSet<string>> GetZones(Empty _, ServerCallContext ctx)
        {
            Logger.Log("GetZones");
            return ProcessTask<HashSet<string>, GetZonesRequestMessage, GetZonesResponseMessage>(
	            new GetZonesRequestMessage(GetType(), typeof(ZMIModule)));
        }

        private Task<AttributesMap> GetAttributes(string pathName, ServerCallContext ctx)
        {
            Logger.Log($"GetAttributes({pathName})");
            return ProcessTask<AttributesMap, GetAttributesRequestMessage, GetAttributesResponseMessage>(
	            new GetAttributesRequestMessage(GetType(), typeof(ZMIModule), pathName));
        }

        private Task<HashSet<string>> GetQueries(Empty _, ServerCallContext ctx)
		{
			Logger.Log($"GetQueries");
			return ProcessTask<HashSet<string>, GetQueriesRequestMessage, GetQueriesResponseMessage>(
				new GetQueriesRequestMessage(GetType(), typeof(ZMIModule)));
		}

		private Task<RefStruct<bool>> InstallQuery(SignedQuery query, ServerCallContext ctx)
		{
			Logger.Log($"InstallQuery");
			return ProcessTask<RefStruct<bool>, InstallQueryRequestMessage, InstallQueryResponseMessage>(
				new InstallQueryRequestMessage(GetType(), typeof(ZMIModule), query));
		}

		private Task<RefStruct<bool>> UninstallQuery(UnsignQuery unsignRequest, ServerCallContext ctx)
		{
			Logger.Log($"UninstallQuery");

			return ProcessTask<RefStruct<bool>, UninstallQueryRequestMessage, UninstallQueryResponseMessage>(
				new UninstallQueryRequestMessage(GetType(), typeof(ZMIModule), unsignRequest));
		}

		private Task<RefStruct<bool>> SetAttribute(AttributeMessage attributeMessage, ServerCallContext ctx)
		{
			Logger.Log($"SetAttribute({attributeMessage})");
			return ProcessTask<RefStruct<bool>, SetAttributeRequestMessage, SetAttributeResponseMessage>(
				new SetAttributeRequestMessage(GetType(), typeof(ZMIModule), attributeMessage));
		}

		private Task<RefStruct<bool>> SetContacts(ValueSet contacts, ServerCallContext ctx)
		{
			return ProcessTask<RefStruct<bool>, SetContactsRequestMessage, SetContactsResponseMessage>(
				new SetContactsRequestMessage(GetType(), typeof(ZMIModule), contacts));
		}
		
		private Task<T> ProcessTask<T, TReq, TRes>(TReq requestMsg)
			where T : class
			where TReq : IZMIRequestMessage
			where TRes : IZMIResponseMessage<T>
		{
			_executor.AddMessage(requestMsg);

			IMessage responseMessage;

			lock (_dictLock)
			{
				while (!_dictionary.TryGetValue(requestMsg, out responseMessage))
					Monitor.Wait(_dictLock);
			}

			if (!(responseMessage is TRes responseWrapper))
			{
				Logger.LogError("Return message is not a GetQueriesResponseMessage");
				return Task.FromResult(default(T));
			}
	        
			return Task.FromResult(responseWrapper.Response);
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
