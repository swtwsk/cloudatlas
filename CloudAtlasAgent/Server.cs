using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Shared;
using Shared.Model;
using Shared.Parsers;
using Shared.RPC;

namespace CloudAtlasAgent
{
	public class Server
	{
		private static ServerPort DefaultPort = new ServerPort("127.0.0.1", 5000, ServerCredentials.Insecure);

		private static ZMI _zmi;
		
		private static Dictionary<string, string> _queries = new Dictionary<string, string>();
		private static ValueSet _contacts = new ValueSet(AttributeTypePrimitive.Contact);

		public static void Main(string[] args)
		{
			var serverPort = DefaultPort;
			if (args.Length == 2)
				serverPort = new ServerPort(args[0], int.Parse(args[1]), ServerCredentials.Insecure);

			var scanner = Console.In;
			//_zmi = CreateTestHierarchy();
			if (!ZMIParser.TryParseZMI(scanner, ref _zmi))
			{
				Console.WriteLine("ERROR: Couldn't parse ZMI");
				return;
			}

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
			Console.WriteLine($"GetAttributes({pathName})");
			return Task.FromResult(_zmi.TrySearch(pathName, out var zmi) ? zmi.Attributes : null);
		}

		private static Task<RefStruct<bool>> InstallQuery(string query, ServerCallContext ctx)
		{
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
			Console.WriteLine($"UninstallQuery({queryName})");
			return Task.FromResult(new RefStruct<bool>(_queries.Remove(queryName)));
		}

		private static Task<RefStruct<bool>> SetAttribute(AttributeMessage attributeMessage, ServerCallContext ctx)
		{
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

		#region Debug

		private static ValueContact CreateContact(string path, byte ip1, byte ip2, byte ip3, byte ip4)
		{
			return new ValueContact(new PathName(path), new IPAddress(new[]
			{
				ip1, ip2, ip3, ip4
			}));
		}

		private static ZMI CreateTestHierarchy()
		{
			var violet07Contact = CreateContact("/uw/violet07", 10, 1, 1, 10);
			var khaki13Contact = CreateContact("/uw/khaki13", 10, 1, 1, 38);
			var khaki31Contact = CreateContact("/uw/khaki31", 10, 1, 1, 39);
			var whatever01Contact = CreateContact("/uw/whatever01", 82, 111, 52, 56);
			var whatever02Contact = CreateContact("/uw/whatever02", 82, 111, 52, 57);

			List<Value> list;

			var _root = new ZMI();
			_root.Attributes.Add("level", new ValueInt(0L));
			_root.Attributes.Add("name", new ValueString(null));
			_root.Attributes.Add("owner", new ValueString("/uw/violet07"));
			_root.Attributes.Add("timestamp", new ValueTime("2012/11/09 20:10:17.342"));
			_root.Attributes.Add("contacts", new ValueSet(AttributeTypePrimitive.Contact));
			_root.Attributes.Add("cardinality", new ValueInt(0L));

			var uw = new ZMI(_root);
			_root.AddSon(uw);
			uw.Attributes.Add("level", new ValueInt(1L));
			uw.Attributes.Add("name", new ValueString("uw"));
			uw.Attributes.Add("owner", new ValueString("/uw/violet07"));
			uw.Attributes.Add("timestamp", new ValueTime("2012/11/09 20:8:13.123"));
			uw.Attributes.Add("contacts", new ValueSet(AttributeTypePrimitive.Contact));
			uw.Attributes.Add("cardinality", new ValueInt(0L));

			var pjwstk = new ZMI(_root);
			_root.AddSon(pjwstk);
			pjwstk.Attributes.Add("level", new ValueInt(1L));
			pjwstk.Attributes.Add("name", new ValueString("pjwstk"));
			pjwstk.Attributes.Add("owner", new ValueString("/pjwstk/whatever01"));
			pjwstk.Attributes.Add("timestamp", new ValueTime("2012/11/09 20:8:13.123"));
			pjwstk.Attributes.Add("contacts", new ValueSet(AttributeTypePrimitive.Contact));
			pjwstk.Attributes.Add("cardinality", new ValueInt(0L));

			var violet07 = new ZMI(uw);
			uw.AddSon(violet07);
			violet07.Attributes.Add("level", new ValueInt(2L));
			violet07.Attributes.Add("name", new ValueString("violet07"));
			violet07.Attributes.Add("owner", new ValueString("/uw/violet07"));
			violet07.Attributes.Add("timestamp", new ValueTime("2012/11/09 18:00:00.000"));
			list = new Value[]
			{
				khaki31Contact, whatever01Contact
			}.ToList();
			violet07.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			violet07.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[]
			{
				violet07Contact
			}.ToList();
			violet07.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			violet07.Attributes.Add("creation", new ValueTime("2011/11/09 20:8:13.123"));
			violet07.Attributes.Add("cpu_usage", new ValueDouble(0.9));
			violet07.Attributes.Add("num_cores", new ValueInt(3L));
			violet07.Attributes.Add("has_ups", new ValueBoolean(null));
			list = new Value[]
			{
				new ValueString("tola"), new ValueString("tosia")
			}.ToList();
			violet07.Attributes.Add("some_names", new ValueList(list, AttributeTypePrimitive.String));
			violet07.Attributes.Add("expiry", new ValueDuration(13L, 12L, 0L, 0L, 0L));

			var khaki31 = new ZMI(uw);
			uw.AddSon(khaki31);
			khaki31.Attributes.Add("level", new ValueInt(2L));
			khaki31.Attributes.Add("name", new ValueString("khaki31"));
			khaki31.Attributes.Add("owner", new ValueString("/uw/khaki31"));
			khaki31.Attributes.Add("timestamp", new ValueTime("2012/11/09 20:03:00.000"));
			list = new Value[]
			{
				violet07Contact, whatever02Contact
			}.ToList();
			khaki31.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki31.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[]
			{
				khaki31Contact
			}.ToList();
			khaki31.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki31.Attributes.Add("creation", new ValueTime("2011/11/09 20:12:13.123"));
			khaki31.Attributes.Add("cpu_usage", new ValueDouble(null));
			khaki31.Attributes.Add("num_cores", new ValueInt(3L));
			khaki31.Attributes.Add("has_ups", new ValueBoolean(false));
			list = new Value[]
			{
				new ValueString("agatka"), new ValueString("beatka"), new ValueString("celina")
			}.ToList();
			khaki31.Attributes.Add("some_names", new ValueList(list, AttributeTypePrimitive.String));
			khaki31.Attributes.Add("expiry", new ValueDuration(-13L, -11L, 0L, 0L, 0L));

			var khaki13 = new ZMI(uw);
			uw.AddSon(khaki13);
			khaki13.Attributes.Add("level", new ValueInt(2L));
			khaki13.Attributes.Add("name", new ValueString("khaki13"));
			khaki13.Attributes.Add("owner", new ValueString("/uw/khaki13"));
			khaki13.Attributes.Add("timestamp", new ValueTime("2012/11/09 21:03:00.000"));
			list = new Value[] { }.ToList();
			khaki13.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki13.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[]
			{
				khaki13Contact
			}.ToList();
			khaki13.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki13.Attributes.Add("creation", new ValueTime((RefStruct<long>) null));
			khaki13.Attributes.Add("cpu_usage", new ValueDouble(0.1));
			khaki13.Attributes.Add("num_cores", new ValueInt(null));
			khaki13.Attributes.Add("has_ups", new ValueBoolean(true));
			list = new Value[] { }.ToList();
			khaki13.Attributes.Add("some_names", new ValueList(list, AttributeTypePrimitive.String));
			khaki13.Attributes.Add("expiry", new ValueDuration((RefStruct<long>) null));

			var whatever01 = new ZMI(pjwstk);
			pjwstk.AddSon(whatever01);
			whatever01.Attributes.Add("level", new ValueInt(2L));
			whatever01.Attributes.Add("name", new ValueString("whatever01"));
			whatever01.Attributes.Add("owner", new ValueString("/uw/whatever01"));
			whatever01.Attributes.Add("timestamp", new ValueTime("2012/11/09 21:12:00.000"));
			list = new Value[]
			{
				violet07Contact, whatever02Contact
			}.ToList();
			whatever01.Attributes.Add("contacts",
				new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever01.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[]
			{
				whatever01Contact
			}.ToList();
			whatever01.Attributes.Add("members",
				new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever01.Attributes.Add("creation", new ValueTime("2012/10/18 07:03:00.000"));
			whatever01.Attributes.Add("cpu_usage", new ValueDouble(0.1));
			whatever01.Attributes.Add("num_cores", new ValueInt(7L));
			list = new Value[]
			{
				new ValueString("rewrite")
			}.ToList();
			whatever01.Attributes.Add("php_modules", new ValueList(list, AttributeTypePrimitive.String));

			ZMI whatever02 = new ZMI(pjwstk);
			pjwstk.AddSon(whatever02);
			whatever02.Attributes.Add("level", new ValueInt(2L));
			whatever02.Attributes.Add("name", new ValueString("whatever02"));
			whatever02.Attributes.Add("owner", new ValueString("/uw/whatever02"));
			whatever02.Attributes.Add("timestamp", new ValueTime("2012/11/09 21:13:00.000"));
			list = new Value[]
			{
				khaki31Contact, whatever01Contact
			}.ToList();
			whatever02.Attributes.Add("contacts",
				new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever02.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[]
			{
				whatever02Contact
			}.ToList();
			whatever02.Attributes.Add("members",
				new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever02.Attributes.Add("creation", new ValueTime("2012/10/18 07:04:00.000"));
			whatever02.Attributes.Add("cpu_usage", new ValueDouble(0.4));
			whatever02.Attributes.Add("num_cores", new ValueInt(13L));
			list = new Value[]
			{
				new ValueString("odbc")
			}.ToList();
			whatever02.Attributes.Add("php_modules", new ValueList(list, AttributeTypePrimitive.String));

			return _root;
		}

		#endregion
	}
}