using System;
using System.IO;
using System.Security.Cryptography;
using CommandLine;
using Grpc.Core;

namespace QuerySigner
{
    class QuerySigner
    {
        class Options
        {
            [Option("generate", HelpText = "Generate new RSA keys", SetName = "gen", Required = true)]
            public bool Generate { get; set; }
            
            [Option("genpath", HelpText = "Path prefix for generating RSA keys", SetName = "gen", Required = true)]
            public string GenPathPrefix { get; set; }
            
            [Option('k', "key", Default = null, HelpText = "Path to private key", SetName = "keys", Required = true)]
            public string PrivateKeyPath { get; set; }
            
            [Option('h', "host", Default = "127.0.0.1", HelpText = "Query signer host name")]
            public string ServerHostName { get; set; }
			
            [Option('p', "port", Default = 6666, HelpText = "Query signer port number")]
            public int ServerPortNumber { get; set; }
        }
        
        static void Main(string[] args)
        {
            var serverHostName = "";
            var serverPortNumber = 0;
            RSA rsa = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    serverHostName = opts.ServerHostName;
                    serverPortNumber = opts.ServerPortNumber;

                    if (opts.Generate)
                    {
                        var (privateKey, publicKey) = GenerateKeys();
                        SaveKeys(privateKey, publicKey, opts.GenPathPrefix);
                        Environment.Exit(0);
                    }

                    rsa = FromKey(opts.PrivateKeyPath);
                })
                .WithNotParsed(errs =>
                {
                    foreach (var err in errs)
                        Console.WriteLine($"OPTIONS PARSE ERROR: {err}");
                    Environment.Exit(1);
                });

            var serverPort = new ServerPort(serverHostName, serverPortNumber, ServerCredentials.Insecure);
            var queryServer = new QueryServer(serverPort, rsa);
            Console.WriteLine($"Server started under [{serverPort.Host}:{serverPort.Port}]. Press Enter to stop it...");
            Console.ReadLine();
            queryServer.Dispose();
        }

        private const int RSA_SIZE = 1024;
        
        private static (string privateKey, string publicKey) GenerateKeys()
        {
            using var rsa = RSA.Create(RSA_SIZE);
            var privateKey = rsa.ExportParameters(true);
            var publicKey = rsa.ExportParameters(false);
            
            var stringWriter = new StringWriter();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, publicKey);
            var publicStr = stringWriter.ToString();
            
            stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, privateKey);
            var privateStr = stringWriter.ToString();

            stringWriter.Dispose();
            
            return (privateStr, publicStr);
        }

        private static void SaveKeys(string privateKey, string publicKey, string generatePrefixPath)
        {
            using (var file = File.Create(generatePrefixPath + "rsa.pub"))
            {
                using var stream = new StreamWriter(file);
                stream.Write(publicKey);
                stream.Close();
            }

            using (var file = File.Create(generatePrefixPath + "rsa.private"))
            {
                using var stream = new StreamWriter(file);
                stream.Write(privateKey);
                stream.Close();
            }
        }

        private static RSA FromKey(string privatePath)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            var privateInput = File.ReadAllText(privatePath);
            rsa.FromXmlString(privateInput);
            return rsa;
        }
    }
}
