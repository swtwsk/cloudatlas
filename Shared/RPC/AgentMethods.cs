using System.Collections.Generic;
using Grpc.Core;
using Shared.Model;
using Shared.Serializers;
using Attribute = Shared.Model.Attribute;

namespace Shared.RPC
{
    public static class AgentMethods
    {
        private const string SERVICE_NAME = "CloudAtlasAgent";

        public static Method<Empty, HashSet<string>> GetZones { get; } =
            MethodsUtils.GetMethod<Empty, HashSet<string>>(MethodType.Unary, SERVICE_NAME, "GetZones",
                CustomSerializer.Serializer);

        public static Method<string, AttributesMap> GetAttributes { get; } =
            MethodsUtils.GetMethod<string, AttributesMap>(MethodType.Unary, SERVICE_NAME, "GetAttributes",
                CustomSerializer.Serializer);

        public static Method<Empty, HashSet<string>> GetQueries { get; } =
            MethodsUtils.GetMethod<Empty, HashSet<string>>(MethodType.Unary, SERVICE_NAME, "GetQueries",
                CustomSerializer.Serializer);

        public static Method<SignedQuery, RefStruct<bool>> InstallQuery { get; } =
            MethodsUtils.GetMethod<SignedQuery, RefStruct<bool>>(MethodType.Unary, SERVICE_NAME, "InstallQuery",
                CustomSerializer.Serializer);

        public static Method<string, RefStruct<bool>> UninstallQuery { get; } =
            MethodsUtils.GetMethod<string, RefStruct<bool>>(MethodType.Unary, SERVICE_NAME, "UninstallQuery",
                CustomSerializer.Serializer);
        
        public static Method<AttributeMessage, RefStruct<bool>> SetAttribute { get; } =
            MethodsUtils.GetMethod<AttributeMessage, RefStruct<bool>>(MethodType.Unary, SERVICE_NAME, "SetAttribute",
                CustomSerializer.Serializer);

        public static Method<ValueSet, RefStruct<bool>> SetContacts { get; } =
            MethodsUtils.GetMethod<ValueSet, RefStruct<bool>>(MethodType.Unary, SERVICE_NAME, "SetContacts",
                CustomSerializer.Serializer);
    }

    public class AttributeMessage
    {
        public string PathName { get; set; }
        public Attribute Attribute { get; set; }
        public Value Value { get; set; }

        public void Deconstruct(out string pathName, out Attribute attribute, out Value value)
        {
            pathName = PathName;
            attribute = Attribute;
            value = Value;
        }
    }
}