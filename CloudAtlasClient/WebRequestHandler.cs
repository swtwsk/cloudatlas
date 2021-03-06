using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Nancy;
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
        private readonly IServerData _serverData;

        private const int RPC_TIMEOUT_SECONDS = 5;
        
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
                    var signResult = await SignQuery(GetSignerInvoker(), query);
                    if (signResult.SignError != SignError.NoError) // TODO: Handle errors
                    {
                        returnDict[i] = false;
                        continue;
                    }
                    var result = await InstallAsync(GetInvoker(), signResult);
                    returnDict[i] = result;
                }
                var response = (Response) JsonConvert.SerializeObject(returnDict);
                response.ContentType = "application/json";
                return response;
            });
            Delete("/query/{name}", async parameters =>
            {
                string name = parameters.name;
                using var call = GetSignerInvoker()
                    .AsyncUnaryCall(SignerMethods.UnsignQuery, null,
                        new CallOptions(deadline: DateTime.UtcNow.AddSeconds(RPC_TIMEOUT_SECONDS)), name);
                
                var unsignQuery = await call.ResponseAsync;
                if (!unsignQuery.UnsignSuccessful)
                    return false;
                
                var result = await UninstallAsync(GetInvoker(), unsignQuery);
                return Response.AsJson(result);
            });
            Post("/agent", async parameters =>
            {
                var requestBody = Request.Body.AsString();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);
                if (!dict.TryGetValue("host", out var host))
                    return false;
                if (!dict.TryGetInt("port", out var port))
                    return false;

                _serverData.HostName = host;
                _serverData.PortNumber = port;
                
                return true;
            });
        }

        private async Task<SignedQuery> SignQuery(CallInvoker invoker, string query)
        {
            var q = query.Split(":", 2);
            if (q.Length != 2)
                return new SignedQuery {SignError = SignError.IncorrectQuery};

            var name = q[0];
            var innerQuery = q[1];
            using var call = invoker.AsyncUnaryCall(SignerMethods.SignQuery, null,
                new CallOptions(deadline: DateTime.UtcNow.AddSeconds(RPC_TIMEOUT_SECONDS)),
                new SignRequest {Name = name, Query = innerQuery});
            return await call.ResponseAsync;
        }

        private DefaultCallInvoker GetInvoker()
        {
            var channel = new Channel(_serverData.HostName, _serverData.PortNumber, ChannelCredentials.Insecure);
            return new DefaultCallInvoker(channel);
        }
        
        private DefaultCallInvoker GetSignerInvoker()
        {
            var channel = new Channel(_serverData.SignerHostName, _serverData.SignerPortNumber, ChannelCredentials.Insecure);
            return new DefaultCallInvoker(channel);
        }
        
        private static async Task<HashSet<string>> GetZones(CallInvoker invoker)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetZones, null,
                new CallOptions(deadline: DateTime.UtcNow.AddSeconds(RPC_TIMEOUT_SECONDS)), new Empty());
            return await call.ResponseAsync;
        }
        
        private static async Task<Dictionary<string, IsNumericResponse>> GetAttributes(CallInvoker invoker, string pathName)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetAttributes, null,
                new CallOptions(deadline: DateTime.UtcNow.AddSeconds(RPC_TIMEOUT_SECONDS)), pathName);
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
            using var call = invoker.AsyncUnaryCall(AgentMethods.GetQueries, null,
                new CallOptions(deadline: DateTime.UtcNow.AddSeconds(RPC_TIMEOUT_SECONDS)), new Empty());
            return await call.ResponseAsync;
        }
        
        private static async Task<bool> InstallAsync(CallInvoker invoker, SignedQuery query)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.InstallQuery, null,
                new CallOptions(deadline: DateTime.UtcNow.AddSeconds(RPC_TIMEOUT_SECONDS)), query);
            var result = await call.ResponseAsync;
            return result.Ref;
        }
        
        private static async Task<bool> UninstallAsync(CallInvoker invoker, UnsignQuery unsignResponse)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.UninstallQuery, null,
                new CallOptions(deadline: DateTime.UtcNow.AddSeconds(RPC_TIMEOUT_SECONDS)), unsignResponse);
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