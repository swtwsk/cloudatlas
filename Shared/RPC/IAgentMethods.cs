using System.Collections.Generic;
using Grpc.Core;
using Shared.Model;
using Attribute = Shared.Model.Attribute;

namespace Shared.RPC
{
    public interface IAgentMethods
    {
        Method<Empty, List<string>> GetZones { get; }
        Method<string, AttributesMap> GetAttributes { get; }
        Method<string, Empty> InstallQuery { get; }
        Method<string, Empty> UninstallQuery { get; }
        Method<AttributeMessage, Empty> SetAttribute { get; }
        Method<ValueSet, Empty> SetContacts { get; }
    }

    public class AttributeMessage
    {
        public string PathName { get; set; }
        public Attribute Attribute { get; set; }
        public Value Value { get; set; }
    }
}