using System.Net;
using Ceras;
using Ceras.Resolvers;
using Shared.Model;

namespace Shared.Serializers
{
    public static class CustomSerializer
    {
        public static CerasSerializer Serializer => new CerasSerializer(SerializerConfig);

        private static SerializerConfig _serializerConfig;
        private static SerializerConfig SerializerConfig
        {
            get
            {
                if (_serializerConfig != null)
                    return _serializerConfig;
                
                _serializerConfig = new SerializerConfig
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
                };
                _serializerConfig.ConfigType<IPAddress>().ConstructByFormatter();
                _serializerConfig.OnResolveFormatter.Add((s, t) =>
                    t == typeof(IPAddress) ? new IPAddressFormatter() : null);
                _serializerConfig.Advanced.DelegateSerialization = DelegateSerializationFlags.AllowStatic | DelegateSerializationFlags.AllowInstance;

                return _serializerConfig;
            }
        }
    }
}