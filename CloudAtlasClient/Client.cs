using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Ceras;
using Ceras.Resolvers;
using Grpc.Core;
using Shared;
using Shared.Model;
using Shared.RPC;
using Shared.Serializers;
using Attribute = System.Attribute;

namespace CloudAtlasClient
{
    class Client
    {
        static void Main(string[] args)
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                if (line == "\n")
                    return;
                RunAsync(line).Wait();
            }
        }
        
        private static async Task RunAsync(string line)
        {
            var channel = new Channel("127.0.0.1", 5000, ChannelCredentials.Insecure);
            var invoker = new DefaultCallInvoker(channel);

            var o = line.Split(" ", 2);
            switch (o[0])
            {
                case "GETZONES":
                    await GetZones(invoker);
                    break;
                case "GETATTS":
                    await GetAttributes(invoker, o[1]);
                    break;
                case "INSTALL":
                    await InstallAsync(invoker, o[1]);
                    break;
                case "UNINSTALL":
                    break;
                case "SETATTR":
                    break;
                case "SETCONTACTS":
                    break;
                default:
                    break;
            }

            await channel.ShutdownAsync();
        }
        
        private static async Task GetZones(CallInvoker invoker)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetZones, null, new CallOptions(), new Empty());
            var result = await call.ResponseAsync;

            Console.WriteLine("GetZones:");
            foreach (var zone in result)
            {
                Console.WriteLine($"  {zone}");
            }
        }
        
        private static async Task GetAttributes(CallInvoker invoker, string pathName)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetAttributes, null, new CallOptions(), pathName);
            var result = await call.ResponseAsync;

            Console.WriteLine("GetAttributes:");
            foreach (var (attribute, value) in result)
            {
                Console.WriteLine($"  {attribute}: {value}");
            }
        }

        private static async Task InstallAsync(CallInvoker invoker, string query)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.InstallQuery, null, new CallOptions(), query);
            var result = await call.ResponseAsync;
            Console.WriteLine($"InstallAsync = {result.Ref}");
        }
    }
}