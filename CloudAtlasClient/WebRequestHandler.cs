using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Nancy;
using Nancy.Configuration;
using Nancy.Conventions;
using Nancy.Extensions;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Shared;
using Shared.Model;
using Shared.RPC;

namespace CloudAtlasClient
{
    public sealed class WebRequestHandler : NancyModule
    {
        private IServerData _serverData;
        
        public WebRequestHandler(IServerData serverData)
        {
            _serverData = serverData;
            
            Get("/", _ => Response.AsFile("dist/index.html", "text/html"));
            Get("/zmis", async _ =>
            {
                var result = await GetZones(GetInvoker());
                return Response.AsJson(result);
            });
            Get("/zmi", async parameters =>
            {
                var result = await GetAttributes(GetInvoker(), "/");
                return Response.AsJson(result);
            });
            Get("/zmi/{path*}", async parameters =>
            {
                string path = parameters.path;
                var result = await GetAttributes(GetInvoker(), path);
                return Response.AsJson(result);
            });
            Get("/query", async _ =>
            {
                var result = await GetQueries(GetInvoker());
                return Response.AsJson(result);
            });
            Post("/query", async parameters =>
            {
                var requestBody = Request.Body.AsString();
                var dict = JsonConvert.DeserializeObject<Dictionary<int, string>>(requestBody);
                var returnDict = new Dictionary<int, bool>();
                foreach (var (i, query) in dict)
                {
                    var result = await InstallAsync(GetInvoker(), query);
                    returnDict[i] = result;
                }
                var response = (Response) JsonConvert.SerializeObject(returnDict);
                response.ContentType = "application/json";
                return response;
            });
            Delete("/query/{name}", async parameters =>
            {
                string name = parameters.name;
                var result = await UninstallAsync(GetInvoker(), name);
                return Response.AsJson(result);
            });
        }

        private DefaultCallInvoker GetInvoker()
        {
            var channel = new Channel(_serverData.HostName, _serverData.PortNumber, ChannelCredentials.Insecure);
            return new DefaultCallInvoker(channel);
        }
        
        private static async Task<HashSet<string>> GetZones(CallInvoker invoker)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetZones, null, new CallOptions(), new Empty());
            return await call.ResponseAsync;
        }
        
        private static async Task<Dictionary<string, IsNumericResponse>> GetAttributes(CallInvoker invoker, string pathName)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetAttributes, null, new CallOptions(), pathName);
            var result = await call.ResponseAsync;

            bool IsNumeric(AttributeType attributeType) =>
                new[] {PrimaryType.Double, PrimaryType.Int}.Contains(attributeType.PrimaryType);

            return result.ToDictionary(
                pair => pair.Key.Name,
                pair => new IsNumericResponse
                {
                    Value = ((ValueString) pair.Value.ConvertTo(AttributeTypePrimitive.String)).Value,
                    IsNumeric = IsNumeric(pair.Value.AttributeType)
                }
            );
        }
        
        private static async Task<HashSet<string>> GetQueries(CallInvoker invoker)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetQueries, null, new CallOptions(), new Empty());
            return await call.ResponseAsync;
        }
        
        private static async Task<bool> InstallAsync(CallInvoker invoker, string query)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.InstallQuery, null, new CallOptions(), query);
            var result = await call.ResponseAsync;
            return result.Ref;
        }
        
        private static async Task<bool> UninstallAsync(CallInvoker invoker, string queryName)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.UninstallQuery, null, new CallOptions(), queryName);
            var result = await call.ResponseAsync;
            return result.Ref;
        }

        private struct IsNumericResponse
        {
            public string Value;
            public bool IsNumeric;
        }
    }
    
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly IServerData _serverData;

        public Bootstrapper(IServerData serverData)
        {
            _serverData = serverData;
        }

        public override void Configure(INancyEnvironment environment)
        {
            base.Configure(environment);
            environment.Tracing(enabled: false, displayErrorTraces: true);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/", @"dist")
            );
            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/js", @"dist/js")
            );
            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("/css", @"dist/css")
            );
            base.ConfigureConventions(nancyConventions);
        }
        
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(_serverData);
        }
    }
}