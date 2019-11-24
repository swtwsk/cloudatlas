using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using Shared;
using Shared.Model;
using Shared.Parsers;
using Shared.RPC;

namespace CloudAtlasAgent
{
	public class Server
	{
		private static ZMI _zmi;
		private static Dictionary<string, string> _queries = new Dictionary<string, string>();
		private static ValueSet _contacts = new ValueSet(AttributeTypePrimitive.Contact);
		private static bool _log = false;
		
		class Options
		{
			[Option('h', "host", Default = "127.0.0.1", HelpText = "Server host name")]
			public string HostName { get; set; }
			
			[Option('p', "port", Default = 5000, HelpText = "Server port number")]
			public int PortNumber { get; set; }
			
			[Option('c', "config", Default = "zmis.txt", HelpText = "ZMI config file path")]
			public string ConfigFile { get; set; }
			
			[Option('l', "log", Default = false, HelpText = "Enable logging of incoming RMIs")]
			public bool Log { get; set; }
		}

		private static bool TryParseConfig(string pathToConfig, out ZMI zmi)
		{
			zmi = null;
			
			if (!File.Exists(pathToConfig))
			{
				Console.Error.WriteLine($"Could not find config file in path: {pathToConfig}");
				return false;
			}

			using var file = File.OpenRead(pathToConfig);
			if (ZMIParser.TryParseZMI(new StreamReader(file), ref zmi))
				return true;
			
			Console.Error.WriteLine($"Could not parse ZMI from file: {pathToConfig}");
			return false;
		}

		public static void Main(string[] args)
		{
			ServerPort serverPort = null;
			
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(opts =>
				{
					if (!TryParseConfig(opts.ConfigFile, out _zmi))
						Environment.Exit(1);
					serverPort = new ServerPort(opts.HostName.Trim(' '), opts.PortNumber, ServerCredentials.Insecure);
					_log = opts.Log;
				})
				.WithNotParsed(errs =>
				{
					foreach (var err in errs)
						Console.WriteLine($"OPTIONS PARSE ERROR: {err}");
					Environment.Exit(1);
				});
			
			RunServer(serverPort).Wait();
		}

		private static async Task RunServer(ServerPort serverPort)
		{
			var server = new Grpc.Core.Server
			{
				Ports = {serverPort},
				Services =
				{
					ServerServiceDefinition.CreateBuilder()
						.AddMethod(AgentMethods.GetZones, GetZones)
						.AddMethod(AgentMethods.GetAttributes, GetAttributes)
						.AddMethod(AgentMethods.GetQueries, GetQueries)
						.AddMethod(AgentMethods.InstallQuery, InstallQuery)
						.AddMethod(AgentMethods.UninstallQuery, UninstallQuery)
						.AddMethod(AgentMethods.SetAttribute, SetAttribute)
						.AddMethod(AgentMethods.SetContacts, SetContacts)
						.Build()
				}
			};

			server.Start();
			Console.WriteLine($"Server started under [{serverPort.Host}:{serverPort.Port}]. Press Enter to stop it...");
			Console.ReadLine();

			await server.ShutdownAsync();
		}
		
		private static Task<HashSet<string>> GetZones(Empty _, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine("GetZones");
			
			var set = new HashSet<string>();
			
			void GetRecursiveZones(ZMI zmi)
			{
				set.Add(zmi.PathName.ToString());
				foreach (var son in zmi.Sons)
					GetRecursiveZones(son);
			}
			
			GetRecursiveZones(_zmi);

			return Task.FromResult(set);
		}

		private static Task<AttributesMap> GetAttributes(string pathName, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine($"GetAttributes({pathName})");
			
			return Task.FromResult(_zmi.TrySearch(pathName, out var zmi) ? zmi.Attributes : null);
		}

		private static Task<HashSet<string>> GetQueries(Empty _, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine("GetQueries");
			
			return Task.FromResult(_queries.Keys.ToHashSet());
		}

		private static Task<RefStruct<bool>> InstallQuery(string query, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine($"InstallQuery({query})");
			
			var q = query.Split(":", 2);
			if (q.Length != 2)
				return Task.FromResult(new RefStruct<bool>(false));

			var name = q[0];
			var innerQueries = q[1];

			if (!_queries.TryAdd(name, innerQueries))
				return Task.FromResult(new RefStruct<bool>(false));

			Interpreter.Interpreter.ExecuteQueries(_zmi, innerQueries, false);

			return Task.FromResult(new RefStruct<bool>(true));
		}

		private static Task<RefStruct<bool>> UninstallQuery(string queryName, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine($"UninstallQuery({queryName})");
			
			return Task.FromResult(new RefStruct<bool>(_queries.Remove(queryName)));
		}

		private static Task<RefStruct<bool>> SetAttribute(AttributeMessage attributeMessage, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine($"SetAttribute({attributeMessage})");
			
			var (pathName, attribute, value) = attributeMessage;
			
			if (!_zmi.TrySearch(pathName, out var zmi))
				return Task.FromResult(new RefStruct<bool>(false));
			
			zmi.Attributes.AddOrChange(attribute, value);
			
			foreach (var query in _queries.Values)
				Interpreter.Interpreter.ExecuteQueries(_zmi, query);
			
			return Task.FromResult(new RefStruct<bool>(true));
		}

		private static Task<RefStruct<bool>> SetContacts(ValueSet contacts, ServerCallContext ctx)
		{
			_contacts = contacts;
			return Task.FromResult(new RefStruct<bool>(true));
		}
	}
}