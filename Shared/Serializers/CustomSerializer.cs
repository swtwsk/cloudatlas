using System.Net;
using Ceras;
using Ceras.Resolvers;
using Shared.Model;

namespace Shared.Serializers
{
    public static class CustomSerializer
    {
        private static CerasSerializer _serializer = null;
        public static CerasSerializer Serializer => _serializer ??= new CerasSerializer(SerializerConfig);

        private static SerializerConfig _serializerConfig;
        public static SerializerConfig SerializerConfig
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

                return _serializerConfig;
            }
        }
    }
}