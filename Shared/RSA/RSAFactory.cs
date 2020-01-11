using System.IO;
using System.Security.Cryptography;

namespace Shared.RSA
{
    public static class RSAFactory
    {
        public static System.Security.Cryptography.RSA FromPrivateKey(string privatePath)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            var privateInput = File.ReadAllText(privatePath);
            rsa.FromXmlString(privateInput);
            return rsa;
        }

        public static System.Security.Cryptography.RSA FromPublicKey(string publicPath)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            var publicInput = File.ReadAllText(publicPath);
            rsa.FromXmlString(publicInput);
            return rsa;
        }
    }
}