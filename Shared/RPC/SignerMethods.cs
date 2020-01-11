using Grpc.Core;
using Shared.Serializers;

namespace Shared.RPC
{
    public class SignerMethods
    {
        private const string SERVICE_NAME = "CloudAtlasQuerySigner";

        public static Method<SignRequest, SignedQuery> SignQuery { get; } =
            MethodsUtils.GetMethod<SignRequest, SignedQuery>(MethodType.Unary, SERVICE_NAME, "SignQuery",
                CustomSerializer.Serializer);

        public static Method<string, UnsignQuery> UnsignQuery { get; } =
            MethodsUtils.GetMethod<string, UnsignQuery>(MethodType.Unary, SERVICE_NAME, "UnsignQuery",
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
        public SignError SignError { get; set; } 
        public byte[] SerializedData { get; set; }  // typeof = SignRequest
        public byte[] HashSign { get; set; }

        public void Deconstruct(out byte[] serializedData, out byte[] hashSign)
        {
            serializedData = SerializedData;
            hashSign = HashSign;
        }
    }

    public class UnsignQuery
    {
        public bool UnsignSuccessful { get; set; }
        public byte[] SerializedName { get; set; }
        public byte[] HashSign { get; set; }

        public void Deconstruct(out byte[] serializedName, out byte[] hashSign)
        {
            serializedName = SerializedName;
            hashSign = HashSign;
        }
    }

    public enum SignError
    {
        NoError,
        IncorrectQuery,
        ConflictingQuery,
        IncorrectName,
    }
}
