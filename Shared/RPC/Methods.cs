using System.Collections.Generic;
using Ceras;
using Ceras.Resolvers;
using Grpc.Core;
using Shared.Model;

namespace Shared.RPC
{
    public class AgentMethodsMethods : IAgentMethods
    {
        public const string ServiceName = "CloudAtlasAgent";
        
        private static CerasSerializer _serializer = null;
        public static CerasSerializer Serializer => _serializer ??= new CerasSerializer(new SerializerConfig
        {
            OnConfigNewType = t =>
            {
                if (t.Type.IsSubclassOf(typeof(Value)))
                {
                    t.CustomResolver = (c, tp) => c.Advanced
                        .GetFormatterResolver<DynamicObjectFormatterResolver>()
                        .GetFormatter(tp);
                }
            }
        });

        public Method<Empty, List<string>> GetZones { get; } =
            MethodsUtils.GetMethod<Empty, List<string>>(MethodType.Unary, ServiceName, "GetZones", Serializer);

        public Method<string, AttributesMap> GetAttributes { get; } =
            MethodsUtils.GetMethod<string, AttributesMap>(MethodType.Unary, ServiceName, "GetAttributes", Serializer);

        public Method<string, Empty> InstallQuery { get; } =
            MethodsUtils.GetMethod<string, Empty>(MethodType.Unary, ServiceName, "InstallQuery", Serializer);

        public Method<string, Empty> UninstallQuery { get; } =
            MethodsUtils.GetMethod<string, Empty>(MethodType.Unary, ServiceName, "UninstallQuery", Serializer);


        public Method<AttributeMessage, Empty> SetAttribute { get; } =
            MethodsUtils.GetMethod<AttributeMessage, Empty>(MethodType.Unary, ServiceName, "SetAttribute", Serializer);


        public Method<ValueSet, Empty> SetContacts { get; } =
            MethodsUtils.GetMethod<ValueSet, Empty>(MethodType.Unary, ServiceName, "SetContacts", Serializer);
    }
}
