using Grpc.Core;
using Shared.Monads;
using Shared.Serializers;

namespace Shared.RPC
{
    using SignResponse = Either<RefStruct<SignError>, SignedQuery>;
    
    public class SignerMethods
    {
        private const string SERVICE_NAME = "CloudAtlasQuerySigner";

        public static Method<SignRequest, SignResponse> SignQuery { get; } =
            MethodsUtils.GetMethod<SignRequest, SignResponse>(MethodType.Unary, SERVICE_NAME, "SignQuery",
                CustomSerializer.Serializer);

        public static Method<string, RefStruct<bool>> UnsignQuery { get; } =
            MethodsUtils.GetMethod<string, RefStruct<bool>>(MethodType.Unary, SERVICE_NAME, "UnsignQuery",
                CustomSerializer.Serializer);
    }

    public class SignRequest
    {
        public string Query { get; set; }
        public string Name { get; set; }

        public void Deconstruct(out string query, out string name)
        {
            query = Query;
            name = Name;
        }
    }
    
    public class SignedQuery
    {
        public byte[] SerializedData { get; set; }
        public byte[] HashSign { get; set; }

        public void Deconstruct(out byte[] serializedData, out byte[] hashSign)
        {
            serializedData = SerializedData;
            hashSign = HashSign;
        }
    }

    public enum SignError
    {
        IncorrectQuery,
        ConflictingQuery,
        IncorrectName,
    }
}
