using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CloudAtlasAgent.Modules.Messages;
using CloudAtlasAgent.Modules.Messages.ZMIMessages;
using Shared.Logger;
using Shared.Model;

namespace CloudAtlasAgent.Modules
{
    public class ZMIModule : IModule
    {
        private ZMI _zmi;
        private readonly object _zmiLock = new object();
        private readonly IExecutor _executor;
        private readonly Dictionary<string, string> _queries = new Dictionary<string, string>();
        private readonly object _queriesLock = new object();
        private ValueSet _contacts = new ValueSet(AttributeTypePrimitive.Contact);
        private readonly object _contactsLock = new object();

        public ZMIModule(ZMI zmi, IExecutor executor)
        {
            _zmi = zmi;
            _executor = executor;
            _gossipProcessor = new Thread(ProcessGossipedMessage);
            _gossipProcessor.Start();
        }

        public bool Equals(IModule other) => other is ZMIModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as ZMIModule);
        public override int GetHashCode() => "ZMI".GetHashCode();

        private readonly Thread _gossipProcessor;
        private readonly BlockingCollection<List<(PathName, AttributesMap)>> _gossipedMessages =
	        new BlockingCollection<List<(PathName, AttributesMap)>>();

        public void Dispose()
        {
	        _gossipProcessor?.Interrupt();
	        _gossipedMessages?.Dispose();
        }

        private void ProcessGossipedMessage()
        {
	        try
	        {
		        while (true)
		        {
			        var gossip = _gossipedMessages.Take();
			        Logger.Log($"Processing gossiped message :)\n");
			        
			        lock (_zmiLock)
				        _zmi.UpdateZMI(gossip);
		        }
	        }
	        catch (ThreadInterruptedException) {}
	        catch (ObjectDisposedException) {}
	        catch (Exception e) { Logger.LogException(e); }
        }

        public void HandleMessage(IMessage message)
        {
	        if (message is ZMIAskMessage zmiAskMessage)
	        {
		        lock (_zmiLock)
			        _executor.AddMessage(new ZMIResponseMessage(GetType(), zmiAskMessage.Source, _zmi,
				        _contacts.Select(v => v as ValueContact).Where(v => v != null).ToList(), zmiAskMessage.Guid));
		        return;
	        }

	        if (message is ZMIProcessGossipedMessage gossipedMessage)
	        {
		        _gossipedMessages.Add(gossipedMessage.Gossiped);
		        return;
	        }
	        
	        switch (message as IZMIRequestMessage)
            {
	            case GetAttributesRequestMessage getAttributesZmiMessage:
                    GetAttributes(getAttributesZmiMessage);
                    break;
                case GetQueriesRequestMessage getQueriesRequestMessage:
	                GetQueries(getQueriesRequestMessage);
                    break;
                case GetZonesRequestMessage getZonesZmiMessage:
                    GetZones(getZonesZmiMessage);
                    break;
                case InstallQueryRequestMessage installQueryRequestMessage:
	                InstallQuery(installQueryRequestMessage);
                    break;
                case SetAttributeRequestMessage setAttributeRequestMessage:
	                SetAttribute(setAttributeRequestMessage);
                    break;
                case SetContactsRequestMessage setContactsRequestMessage:
	                SetContacts(setContactsRequestMessage);
                    break;
                case UninstallQueryRequestMessage uninstallQueryRequestMessage:
	                UninstallQuery(uninstallQueryRequestMessage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        private void GetZones(GetZonesRequestMessage requestMessage)
        {
            Logger.Log("GetZones");

            var set = new HashSet<string>();
			
            void GetRecursiveZones(ZMI zmi)
            {
                set.Add(zmi.PathName.ToString());
                foreach (var son in zmi.Sons)
                    GetRecursiveZones(son);
            }
            
            lock(_zmiLock)
                GetRecursiveZones(_zmi.GetFather());

            _executor.AddMessage(new GetZonesResponseMessage(GetType(), requestMessage.Source, requestMessage, set));
        }

        private void GetAttributes(GetAttributesRequestMessage requestMessage)
        {
            Logger.Log($"GetAttributes({requestMessage.PathName})");

            AttributesMap toReturn;
            lock (_zmiLock)
                toReturn = _zmi.GetFather().TrySearch(requestMessage.PathName, out var zmi) ? zmi.Attributes : null;

            _executor.AddMessage(new GetAttributesResponseMessage(GetType(), requestMessage.Source, requestMessage,
                toReturn));
        }
        
        private void GetQueries(GetQueriesRequestMessage requestMessage)
		{
			Logger.Log("GetQueries");

			HashSet<string> toReturn;
			lock (_queriesLock)
				toReturn = _queries.Keys.ToHashSet();

			_executor.AddMessage(new GetQueriesResponseMessage(GetType(), requestMessage.Source, requestMessage, toReturn));
		}

        private void InstallQuery(InstallQueryRequestMessage requestMessage)
        {
	        Logger.Log($"InstallQuery({requestMessage.Query})");

	        var q = requestMessage.Query.Split(":", 2);
	        if (q.Length != 2)
	        {
		        _executor.AddMessage(new InstallQueryResponseMessage(GetType(), requestMessage.Source, requestMessage, 
			        false));
		        return;
	        }

	        var name = q[0];
	        var innerQueries = q[1];

	        bool queryExecuted;

	        // I need to keep this lock that long (unfortunately), otherwise I would not be able to fallback
	        lock (_queriesLock)
	        {
		        if (!_queries.TryAdd(name, innerQueries))
		        {
			        _executor.AddMessage(new InstallQueryResponseMessage(GetType(), requestMessage.Source,
				        requestMessage,
				        false));
			        return;
		        }

		        lock (_zmiLock)
		        {
			        try
			        {
				        Interpreter.Interpreter.ExecuteQueries(_zmi.GetFather(), innerQueries);
				        queryExecuted = true;
			        }
			        catch (Exception e)
			        {
				        Logger.LogException(e);
				        queryExecuted = false;
			        }
		        }

		        // The Fallback
		        if (!queryExecuted)
			        _queries.Remove(name);
	        }

	        _executor.AddMessage(new InstallQueryResponseMessage(GetType(), requestMessage.Source, requestMessage, queryExecuted));
        }

        private void UninstallQuery(UninstallQueryRequestMessage requestMessage)
		{
			Logger.Log($"UninstallQuery({requestMessage.QueryName})");

			bool toReturn;
			lock (_queriesLock)
				toReturn = _queries.Remove(requestMessage.QueryName);
			_executor.AddMessage(
				new UninstallQueryResponseMessage(GetType(), requestMessage.Source, requestMessage, toReturn));
		}

		private void SetAttribute(SetAttributeRequestMessage requestMessage)
		{
			Logger.Log($"SetAttribute({requestMessage.AttributeMessage})");
			
			var (pathName, attribute, value) = requestMessage.AttributeMessage;

			lock (_zmiLock)
			{
				if (!_zmi.GetFather().TrySearch(pathName, out var zmi))
					_executor.AddMessage(new SetAttributeResponseMessage(GetType(), requestMessage.Source, requestMessage,
						false));

				zmi.Attributes.AddOrChange(attribute, value);

				lock (_queriesLock)
				{
					foreach (var query in _queries.Values)
						Interpreter.Interpreter.ExecuteQueries(_zmi.GetFather(), query);
				}
			}

			_executor.AddMessage(new SetAttributeResponseMessage(GetType(), requestMessage.Source, requestMessage, true));
		}

		private void SetContacts(SetContactsRequestMessage requestMessage)
		{
			lock (_contactsLock)
				_contacts = requestMessage.Contacts;
			_executor.AddMessage(new SetContactsResponseMessage(GetType(), requestMessage.Source, requestMessage, true));
		}
    }
}