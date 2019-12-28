using System;
using System.Collections.Generic;
using CloudAtlasAgent.Modules.Messages;
using CloudAtlasAgent.Modules.Messages.ZMIMessages;
using Shared.Logger;
using Shared.Model;

namespace CloudAtlasAgent.Modules
{
    public class ZMIModule : IModule
    {
        private readonly ZMI _zmi;
        private readonly object _zmiLock = new object();
        private readonly IExecutor _executor;

        public ZMIModule(ZMI zmi, IExecutor executor)
        {
            _zmi = zmi;
            _executor = executor;
        }

        public bool Equals(IModule other) => other is ZMIModule;
        public override bool Equals(object? obj) => obj != null && Equals(obj as ZMIModule);
        public override int GetHashCode() => "ZMI".GetHashCode();

        public void Dispose()
        {
        }

        public void HandleMessage(IMessage message)
        {
            switch (message as IZMIRequestMessage)
            {
                case GetAttributesRequestMessage getAttributesZmiMessage:
                    GetAttributes(getAttributesZmiMessage);
                    break;
                case GetZonesRequestMessage getZonesZmiMessage:
                    GetZones(getZonesZmiMessage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                GetRecursiveZones(_zmi);

            _executor.AddMessage(new GetZonesResponseMessage(this, requestMessage.Source, requestMessage, set));
        }

        private void GetAttributes(GetAttributesRequestMessage requestMessage)
        {
            Logger.Log($"GetAttributes({requestMessage.PathName})");

            AttributesMap toReturn;
            lock (_zmiLock)
                toReturn = _zmi.TrySearch(requestMessage.PathName, out var zmi) ? zmi.Attributes : null;

            _executor.AddMessage(new GetAttributesResponseMessage(this, requestMessage.Source, requestMessage,
                toReturn));
        }
    }
}