using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CloudAtlasAgent.Modules;
using CloudAtlasAgent.Modules.Messages;
using CommandLine;
using Grpc.Core;
using Shared;
using Shared.Logger;
using Shared.Model;
using Shared.Parsers;
using Shared.RPC;

namespace CloudAtlasAgent
{
	public class Server
	{
		private static ZMI _zmi;
		private static readonly object _zmiLock = new object();
		private static Dictionary<string, string> _queries = new Dictionary<string, string>();
		private static readonly object _queriesLock = new object();
		private static ValueSet _contacts = new ValueSet(AttributeTypePrimitive.Contact);
		private static readonly object _contactsLock = new object();
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
			
			[Option('d', Default = "127.0.0.1", HelpText = "Destination hostname of test message")]
			public string DestName { get; set; }
			
			[Option('f', Default = 1234, HelpText = "Destination port of test message")]
			public int DestPort { get; set; }
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
			var pair = (string.Empty, -1);
			
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(opts =>
				{
					if (!TryParseConfig(opts.ConfigFile, out _zmi))
						Environment.Exit(1);
					serverPort = new ServerPort(opts.HostName.Trim(' '), opts.PortNumber, ServerCredentials.Insecure);
					pair = (opts.DestName.Trim(' '), opts.DestPort);
					_log = opts.Log;
				})
				.WithNotParsed(errs =>
				{
					foreach (var err in errs)
						Console.WriteLine($"OPTIONS PARSE ERROR: {err}");
					Environment.Exit(1);
				});

			Logger.LoggerLevel = LoggerLevel.All;
			Logger.LoggerVerbosity = LoggerVerbosity.WithFileName;
			destPair = pair;
			TestModule(_zmi, serverPort);
			//RunServer(serverPort).Wait();
		}
		
		static void PrintTest()
		{
			Console.WriteLine($"TEST ME ONLINE");
		}

		static void AddMessage()
		{
			e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), new TimerModule(), 0, 10, DateTimeOffset.Now,
				AddMessage));
			e.AddMessage(new CommunicationSendMessage(new DummyModule(), communication1,
				new TimerAddCallbackMessage(
					new DummyModule(), new TimerModule(), 0, 10, DateTimeOffset.Now, PrintTest),
				IPAddress.Parse(destPair.Item1), destPair.Item2));
		}

		private static CommunicationModule communication1;
		private static Executor e;
		private static (string, int) destPair;

		private static void TestModule(ZMI zmi, ServerPort serverPort)
		{
			Console.WriteLine("Test started...");
			using var er = new ExecutorRegistry();
			e = new Executor(er);
			using var er2 = new ExecutorRegistry();
			var e2 = new Executor(er2);
			var timer = new TimerModule();
			if (!e.TryAddModule(timer))
				Console.WriteLine("Could not add TimerModule");
			
			var zmiModule = new ZMIModule(zmi, e);
			if (!e.TryAddModule(zmiModule))
				Console.WriteLine("Could not add ZMI module");
			
			var rmiModule = new RMIModule(e, zmiModule, serverPort);
			if (!e.TryAddModule(rmiModule))
				Console.WriteLine("Could not add RMI module");
			
			communication1 = new CommunicationModule(e, 3000, IPAddress.Parse(serverPort.Host), serverPort.Port + 1, 5000);
			if (!e.TryAddModule(communication1))
				Console.WriteLine("Could not add Communication 1");
			
			e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), timer, 0, 1, DateTimeOffset.Now, 
				AddMessage));
			
			// e2.AddMessage(new CommunicationSendMessage(new DummyModule(), communication2,
			// 	new TimerAddCallbackMessage(new DummyModule(), timer, 0, 2, DateTimeOffset.Now,
			// 		PrintTest), local, 1234));
			// e2.AddMessage(new CommunicationSendMessage(new DummyModule(), communication2,
			// 	new TimerAddCallbackMessage(new DummyModule(), timer, 0, 6, DateTimeOffset.Now,
			// 		PrintTest), local, 1234));
			// e2.AddMessage(new CommunicationSendMessage(new DummyModule(), communication2,
			// 	new TimerAddCallbackMessage(new DummyModule(), timer, 0, 4, DateTimeOffset.Now,
			// 		PrintTest), local, 1234));
			//
			// Thread.Sleep(5000);
			// e2.AddMessage(new CommunicationSendMessage(new DummyModule(), communication2,
			// 	new TimerAddCallbackMessage(new DummyModule(), timer, 0, 2, DateTimeOffset.Now,
			// 		PrintTest), local, 1234));
			
			// e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), timer, 0, 8, DateTimeOffset.Now,
			// 	() => Console.WriteLine("TEST ME 0")));
			// e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), timer, 1, 8, DateTimeOffset.Now,
			// 	() => Console.WriteLine("TEST ME 1")));
			// e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), timer, 2, 1, DateTimeOffset.Now,
			// 	() => Console.WriteLine("TEST ME 2")));
			// e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), timer, 3, 4, DateTimeOffset.Now,
			// 	() => Console.WriteLine("TEST ME 3")));
			// e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), timer, 4, 2, DateTimeOffset.Now,
			// 	() => Console.WriteLine("TEST ME 4")));
			// e.AddMessage(new TimerAddCallbackMessage(new DummyModule(), timer, 5, 4, DateTimeOffset.Now,
			// 	() => Console.WriteLine("TEST ME 5")));
			// e.AddMessage(new TimerRemoveCallbackMessage(new DummyModule(), timer, 3));

			/*if (e.TryAddModule(timer))
			{
				Console.WriteLine("Could add TimerModule (sic!)");
			}*/

			Console.ReadLine();
			Console.WriteLine("End");
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
			
			lock(_zmiLock)
				GetRecursiveZones(_zmi);

			return Task.FromResult(set);
		}

		private static Task<AttributesMap> GetAttributes(string pathName, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine($"GetAttributes({pathName})");

			AttributesMap toReturn;
			lock (_zmiLock)
			{
				toReturn = _zmi.TrySearch(pathName, out var zmi) ? zmi.Attributes : null;
			}
			return Task.FromResult(toReturn);
		}

		private static Task<HashSet<string>> GetQueries(Empty _, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine("GetQueries");

			HashSet<string> toReturn;
			lock (_queriesLock)
				toReturn = _queries.Keys.ToHashSet();
			
			return Task.FromResult(toReturn);
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

			lock (_zmiLock)
				Interpreter.Interpreter.ExecuteQueries(_zmi, innerQueries, false);

			return Task.FromResult(new RefStruct<bool>(true));
		}

		private static Task<RefStruct<bool>> UninstallQuery(string queryName, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine($"UninstallQuery({queryName})");

			bool toReturn;
			lock (_queriesLock)
				toReturn = _queries.Remove(queryName);
			
			return Task.FromResult(new RefStruct<bool>(toReturn));
		}

		private static Task<RefStruct<bool>> SetAttribute(AttributeMessage attributeMessage, ServerCallContext ctx)
		{
			if (_log)
				Console.WriteLine($"SetAttribute({attributeMessage})");
			
			var (pathName, attribute, value) = attributeMessage;

			lock (_zmiLock)
			{
				if (!_zmi.TrySearch(pathName, out var zmi))
					return Task.FromResult(new RefStruct<bool>(false));

				zmi.Attributes.AddOrChange(attribute, value);

				lock (_queriesLock)
				{
					foreach (var query in _queries.Values)
						Interpreter.Interpreter.ExecuteQueries(_zmi, query);
				}
			}

			return Task.FromResult(new RefStruct<bool>(true));
		}

		private static Task<RefStruct<bool>> SetContacts(ValueSet contacts, ServerCallContext ctx)
		{
			lock (_contactsLock)
				_contacts = contacts;
			return Task.FromResult(new RefStruct<bool>(true));
		}
	}
}
