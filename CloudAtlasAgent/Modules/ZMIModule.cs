using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using CloudAtlasAgent.Modules.Messages;
using CloudAtlasAgent.Modules.Messages.ZMIMessages;
using Shared.Logger;
using Shared.Model;
using Shared.RPC;
using Shared.Serializers;

namespace CloudAtlasAgent.Modules
{
    public class ZMIModule : IModule
    {
        private readonly ZMI _zmi;
        private readonly IExecutor _executor;
        private readonly RSA _rsa;
	        
	    private readonly Dictionary<string, string> _queries = new Dictionary<string, string>();
	    private ValueSet _contacts = new ValueSet(AttributeTypePrimitive.Contact);
	    
        private readonly int _recomputeTimer;
        private readonly int _purgeTimer;
        private int _timerRequestId = 0;

        private const string CARDINALITY_QUERY = "SELECT sum(cardinality) AS cardinality";
        
        public ZMIModule(ZMI zmi, RSA rsa, int recomputeTimer, int purgeTimer, IExecutor executor)
        {
            _zmi = zmi;
            _rsa = rsa;
            _recomputeTimer = recomputeTimer;
            _purgeTimer = purgeTimer;
            _executor = executor;
            
            PrepareZMI();
            
            _zmiProcessor = new Thread(ProcessZmiMessage);
            _zmiProcessor.Start();

            _executor.AddMessage(new TimerAddCallbackMessage(GetType(), _timerRequestId++, _recomputeTimer,
	            DateTimeOffset.Now, SendRecomputeQueries));
            _executor.AddMessage(new TimerAddCallbackMessage(GetType(), _timerRequestId++, _purgeTimer,
	            DateTimeOffset.Now, AnnouncePurge));
        }

        private void PrepareZMI()
        {
	        // Add basic attributes
	        _zmi.Attributes.AddOrChange("cardinality", new ValueInt(1));
	        
	        // Add basic queries
	        _queries.Add("cardinality", CARDINALITY_QUERY);
        }
        
        private void SendRecomputeQueries()
        {
	        _executor.AddMessage(new ZMIRecomputeQueriesMessage());
	        _executor.AddMessage(new TimerAddCallbackMessage(GetType(), _timerRequestId++, _recomputeTimer,
		        DateTimeOffset.Now, SendRecomputeQueries));
        }

        private void AnnouncePurge()
        {
	        _executor.AddMessage(new ZMIPurgeMessage());
	        _executor.AddMessage(new TimerAddCallbackMessage(GetType(), _timerRequestId++, _purgeTimer,
		        DateTimeOffset.Now, AnnouncePurge));
        }

        public bool Equals(IModule other) => other is ZMIModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as ZMIModule);
        public override int GetHashCode() => "ZMI".GetHashCode();

        private readonly Thread _zmiProcessor;
        private readonly BlockingCollection<IMessage> _zmiMessages = new BlockingCollection<IMessage>();

        public void Dispose()
        {
	        _zmiProcessor?.Interrupt();
        }

        public void HandleMessage(IMessage message)
        {
	        switch (message)
	        {
		        case ZMIAskMessage _:
		        case IZMIRequestMessage _:
		        case ZMIProcessGossipedMessage _:
		        case ZMIRecomputeQueriesMessage _:
		        case ZMIPurgeMessage _:
			        _zmiMessages.Add(message);
			        return;
		        default:
			        Logger.LogException(new ArgumentOutOfRangeException(nameof(message)));
			        break;
	        }
        }
        
        private void ProcessZmiMessage()
        {
	        try
	        {
		        while (true)
		        {
			        var message = _zmiMessages.Take();
			        switch (message)
			        {
				        case ZMIAskMessage zmiAskMessage:
					        _executor.AddMessage(new ZMIResponseMessage(
						        GetType(),
						        zmiAskMessage.Source,
						        _zmi,
						        _contacts.Select(v => v as ValueContact).Where(v => v != null).ToList(),
						        zmiAskMessage.Guid));
					        continue;
				        case ZMIRecomputeQueriesMessage _:
					        ExecuteQueries();
					        continue;
				        case ZMIProcessGossipedMessage gossipedMessage:
					        Logger.Log($"Processing gossiped message :)\n");
					        _zmi.UpdateZMI(gossipedMessage.Gossiped, gossipedMessage.Delay);
					        continue;
				        case ZMIPurgeMessage _:
					        Logger.Log("The Purge has been announced");
					        var safeMoment =
						        (ValueTime) new ValueTime(DateTimeOffset.Now).Subtract(
							        new ValueDuration(_purgeTimer, 0));
					        Logger.Log($"The safe moment is {safeMoment}");
					        _zmi.GetFather().PurgeTime(safeMoment);
					        Interpreter.Interpreter.ExecuteQueries(_zmi.GetFather(), CARDINALITY_QUERY);
					        _zmi.GetFather().PurgeCardinality();
					        continue;
				        case GetAttributesRequestMessage getAttributesZmiMessage:
					        GetAttributes(getAttributesZmiMessage);
					        continue;
				        case GetQueriesRequestMessage getQueriesRequestMessage:
					        GetQueries(getQueriesRequestMessage);
					        continue;
				        case GetZonesRequestMessage getZonesZmiMessage:
					        GetZones(getZonesZmiMessage);
					        continue;
				        case InstallQueryRequestMessage installQueryRequestMessage:
					        InstallQuery(installQueryRequestMessage);
					        continue;
				        case SetAttributeRequestMessage setAttributeRequestMessage:
					        SetAttribute(setAttributeRequestMessage);
					        continue;
				        case SetContactsRequestMessage setContactsRequestMessage:
					        SetContacts(setContactsRequestMessage);
					        continue;
				        case UninstallQueryRequestMessage uninstallQueryRequestMessage:
					        UninstallQuery(uninstallQueryRequestMessage);
					        continue;
				        default:
					        Logger.LogException(new ArgumentOutOfRangeException(nameof(message)));
					        continue;
			        }
		        }
	        }
	        catch (ThreadInterruptedException) {}
	        catch (ObjectDisposedException) {}
	        catch (Exception e) { Logger.LogException(e); }
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
            
	        GetRecursiveZones(_zmi.GetFather());

            _executor.AddMessage(new GetZonesResponseMessage(GetType(), requestMessage.Source, requestMessage, set));
        }

        private void GetAttributes(GetAttributesRequestMessage requestMessage)
        {
            Logger.Log($"GetAttributes({requestMessage.PathName})");

            var toReturn = _zmi.GetFather().TrySearch(requestMessage.PathName, out var zmi) ? zmi.Attributes : null;

            _executor.AddMessage(new GetAttributesResponseMessage(GetType(), requestMessage.Source, requestMessage,
                toReturn));
        }
        
        private void GetQueries(GetQueriesRequestMessage requestMessage)
		{
			Logger.Log("GetQueries");

			var toReturn = _queries.Keys.ToHashSet();

			_executor.AddMessage(new GetQueriesResponseMessage(GetType(), requestMessage.Source, requestMessage, toReturn));
		}

        private void InstallQuery(InstallQueryRequestMessage requestMessage)
        {
	        Logger.Log($"InstallQuery({requestMessage.Query})");

	        var (serializedData, hashSign) = requestMessage.Query;
	        
	        using var sha256 = SHA256.Create();
	        var hash = sha256.ComputeHash(serializedData);
	        var verified = _rsa.VerifyHash(hash, hashSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	        if (!verified)
	        {
		        _executor.AddMessage(new InstallQueryResponseMessage(GetType(), requestMessage.Source, requestMessage,
			        false));
		        return;
	        }

	        var (innerQueries, name) = CustomSerializer.Serializer.Deserialize<SignRequest>(serializedData);

	        bool queryExecuted;

	        if (!_queries.TryAdd(name, innerQueries))
	        {
		        _executor.AddMessage(new InstallQueryResponseMessage(GetType(), requestMessage.Source,
			        requestMessage, false));
		        // TODO: Remove query from signer
		        return;
	        }

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

	        // The Fallback
	        if (!queryExecuted)
		        _queries.Remove(name);

	        if (queryExecuted)
	        {
		        var updateTimestamp = new ValueTime(DateTimeOffset.Now);
		        _zmi.ApplyUpToFather(z => z.Attributes.AddOrChange("update", updateTimestamp));
	        }

	        _executor.AddMessage(new InstallQueryResponseMessage(GetType(), requestMessage.Source, requestMessage,
		        queryExecuted));
        }

        private void UninstallQuery(UninstallQueryRequestMessage requestMessage)
        {
	        var (serializedData, hashSign) = requestMessage.UnsignRequest;
	        
	        using var sha256 = SHA256.Create();
	        var hash = sha256.ComputeHash(serializedData);
	        var verified = _rsa.VerifyHash(hash, hashSign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

	        if (!verified)
	        {
		        _executor.AddMessage(new UninstallQueryResponseMessage(GetType(), requestMessage.Source, requestMessage,
			        false));
		        return;
	        }
	        
	        var name = CustomSerializer.Serializer.Deserialize<string>(serializedData);
	        
			Logger.Log($"UninstallQuery({name})");

			var toReturn = _queries.Remove(name);
			_executor.AddMessage(
				new UninstallQueryResponseMessage(GetType(), requestMessage.Source, requestMessage, toReturn));
		}

        private void SetAttribute(SetAttributeRequestMessage requestMessage)
        {
	        Logger.Log($"SetAttribute({requestMessage.AttributeMessage})");

	        var (pathName, attribute, value) = requestMessage.AttributeMessage;

	        if (!_zmi.GetFather().TrySearch(pathName, out var zmi))
		        _executor.AddMessage(new SetAttributeResponseMessage(GetType(), requestMessage.Source, requestMessage,
			        false));

	        zmi.Attributes.AddOrChange(attribute, value);

	        // do I even need this?
	        // ExecuteQueries();

	        _executor.AddMessage(
		        new SetAttributeResponseMessage(GetType(), requestMessage.Source, requestMessage, true));
        }

        private void SetContacts(SetContactsRequestMessage requestMessage)
        {
	        _contacts = requestMessage.Contacts;
	        _executor.AddMessage(new SetContactsResponseMessage(GetType(), requestMessage.Source, requestMessage,
		        true));
        }

        private void ExecuteQueries()
        {
	        // TODO: Rethink this
	        var updateTimestamp = new ValueTime(DateTimeOffset.Now);
	        _zmi.ApplyUpToFather(z => z.Attributes.AddOrChange("update", updateTimestamp));

	        foreach (var query in _queries.Values)
		        Interpreter.Interpreter.ExecuteQueries(_zmi.GetFather(), query);
        }
    }
}