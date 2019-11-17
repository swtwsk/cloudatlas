using Ceras;
using Grpc.Core;

namespace Shared.RPC
{
    public static class MethodsUtils
    {
        public static Method<T1, T2> GetMethod<T1, T2>(MethodType methodType, string serviceName, string methodName,
            CerasSerializer serializer) => new Method<T1, T2>(
            methodType,
            serviceName,
            methodName,
            Marshallers.Create(serializer.Serialize, serializer.Deserialize<T1>),
            Marshallers.Create(serializer.Serialize, serializer.Deserialize<T2>));
    }
}
