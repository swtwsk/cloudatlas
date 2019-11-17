using System;
using System.Threading.Tasks;
using Grpc.Core;
using Shared;
using Shared.Model;
using Shared.RPC;
using Attribute = Shared.Model.Attribute;

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
                    await UninstallAsync(invoker, o[1]);
                    break;
                case "SETATTR":
                    var setters = o[1].Split(" ", 3);
                    await SetAttribute(invoker, setters[0], new Attribute(setters[1]),  // TODO: for now
                        new ValueBoolean(bool.Parse(setters[2])));
                    break;
                case "SETCONTACTS":
                    await SetContacts(invoker, new ValueSet(AttributeTypePrimitive.Contact));  // TODO: for now
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
                Console.WriteLine($"  {attribute}: {((ValueString)value.ConvertTo(AttributeTypePrimitive.String)).Value}");
            }
        }

        private static async Task InstallAsync(CallInvoker invoker, string query)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.InstallQuery, null, new CallOptions(), query);
            var result = await call.ResponseAsync;
            Console.WriteLine($"InstallAsync = {result.Ref}");
        }
        
        private static async Task UninstallAsync(CallInvoker invoker, string queryName)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.UninstallQuery, null, new CallOptions(), queryName);
            var result = await call.ResponseAsync;
            Console.WriteLine($"UninstallAsync = {result.Ref}");
        }
        
        private static async Task SetAttribute(CallInvoker invoker, string pathName, Attribute attribute, Value value)
        {
            var attributeMsg = new AttributeMessage {PathName = pathName, Attribute = attribute, Value = value};
            using var call = invoker.AsyncUnaryCall(AgentMethods.SetAttribute, null, new CallOptions(), attributeMsg);
            var result = await call.ResponseAsync;
            Console.WriteLine($"SetAttribute = {result.Ref}");
        }
        
        private static async Task SetContacts(CallInvoker invoker, ValueSet contacts)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.SetContacts, null, new CallOptions(), contacts);
            var result = await call.ResponseAsync;
            Console.WriteLine($"SetContacts = {result.Ref}");
        }
    }
}