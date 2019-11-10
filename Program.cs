using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Antlr4.Runtime;
using CloudAtlas.Interpreter;
using CloudAtlas.Model;
using CloudAtlas.Query;

namespace CloudAtlas
{
	public class Program {
		private static ZMI root;
	
		public static void Main(string[] args) {
			root = CreateTestHierarchy();
			var scanner = Console.In;
			string line;
		
			while ((line = scanner.ReadLine()) != null)
			{
				ExecuteQueries(root, line);
			}
		}
	
		private static PathName GetPathName(ZMI zmi) {
			var name = ((ValueString)zmi.Attributes.Get("name")).Value;
			return zmi.Father == null? PathName.Root : GetPathName(zmi.Father).LevelDown(name);
		}

		private static void ExecuteQueries(ZMI zmi, string query)
		{
			if (!zmi.Sons.Any()) 
				return;
			
			foreach (var son in zmi.Sons)
				ExecuteQueries(son, query);
			
			var lexer = new QueryLexer(new AntlrInputStream(query));
			var parser = new QueryParser(new CommonTokenStream(lexer));
			var result = (List<QueryResult>) new QueryVisitor().Visit(parser.program());
			var zone = GetPathName(zmi);
			
			foreach (var r in result) {
				Console.WriteLine(zone + ": " + r);
				zmi.Attributes.AddOrChange(r.Name, r.Value);
			}
		}

		private static ValueContact CreateContact(string path, byte ip1, byte ip2, byte ip3, byte ip4) {
			return new ValueContact(new PathName(path), new IPAddress(new[] {
				ip1, ip2, ip3, ip4
			}));
		}
	
		private static ZMI CreateTestHierarchy() {
			var violet07Contact = CreateContact("/uw/violet07", 10, 1, 1, 10);
			var khaki13Contact = CreateContact("/uw/khaki13", 10, 1, 1, 38);
			var khaki31Contact = CreateContact("/uw/khaki31", 10, 1, 1, 39);
			var whatever01Contact = CreateContact("/uw/whatever01", 82, 111, 52, 56);
			var whatever02Contact = CreateContact("/uw/whatever02", 82, 111, 52, 57);
		
			List<Value> list;
		
			root = new ZMI();
			root.Attributes.Add("level", new ValueInt(0L));
			root.Attributes.Add("name", new ValueString(null));
			root.Attributes.Add("owner", new ValueString("/uw/violet07"));
			root.Attributes.Add("timestamp", new ValueTime("2012/11/09 20:10:17.342"));
			root.Attributes.Add("contacts", new ValueSet(AttributeTypePrimitive.Contact));
			root.Attributes.Add("cardinality", new ValueInt(0L));
		
			var uw = new ZMI(root);
			root.AddSon(uw);
			uw.Attributes.Add("level", new ValueInt(1L));
			uw.Attributes.Add("name", new ValueString("uw"));
			uw.Attributes.Add("owner", new ValueString("/uw/violet07"));
			uw.Attributes.Add("timestamp", new ValueTime("2012/11/09 20:8:13.123"));
			uw.Attributes.Add("contacts", new ValueSet(AttributeTypePrimitive.Contact));
			uw.Attributes.Add("cardinality", new ValueInt(0L));
		
			var pjwstk = new ZMI(root);
			root.AddSon(pjwstk);
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
			list = new Value[] {
				khaki31Contact, whatever01Contact
			}.ToList();
			violet07.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			violet07.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[] {
				violet07Contact
			}.ToList();
			violet07.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			violet07.Attributes.Add("creation", new ValueTime("2011/11/09 20:8:13.123"));
			violet07.Attributes.Add("cpu_usage", new ValueDouble(0.9));
			violet07.Attributes.Add("num_cores", new ValueInt(3L));
			violet07.Attributes.Add("has_ups", new ValueBoolean(null));
			list = new Value[] {
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
			list = new Value[] {
				violet07Contact, whatever02Contact
			}.ToList();
			khaki31.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki31.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[] {
				khaki31Contact
			}.ToList();
			khaki31.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki31.Attributes.Add("creation", new ValueTime("2011/11/09 20:12:13.123"));
			khaki31.Attributes.Add("cpu_usage", new ValueDouble(null));
			khaki31.Attributes.Add("num_cores", new ValueInt(3L));
			khaki31.Attributes.Add("has_ups", new ValueBoolean(false));
			list = new Value[] {
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
			list = new Value[] {}.ToList();
			khaki13.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki13.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[] {
				khaki13Contact
			}.ToList();
			khaki13.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			khaki13.Attributes.Add("creation", new ValueTime((RefStruct<long>)null));
			khaki13.Attributes.Add("cpu_usage", new ValueDouble(0.1));
			khaki13.Attributes.Add("num_cores", new ValueInt(null));
			khaki13.Attributes.Add("has_ups", new ValueBoolean(true));
			list = new Value[] {}.ToList();
			khaki13.Attributes.Add("some_names", new ValueList(list, AttributeTypePrimitive.String));
			khaki13.Attributes.Add("expiry", new ValueDuration((RefStruct<long>)null));
		
			var whatever01 = new ZMI(pjwstk);
			pjwstk.AddSon(whatever01);
			whatever01.Attributes.Add("level", new ValueInt(2L));
			whatever01.Attributes.Add("name", new ValueString("whatever01"));
			whatever01.Attributes.Add("owner", new ValueString("/uw/whatever01"));
			whatever01.Attributes.Add("timestamp", new ValueTime("2012/11/09 21:12:00.000"));
			list = new Value[] {
				violet07Contact, whatever02Contact
			}.ToList();
			whatever01.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever01.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[] {
				whatever01Contact
			}.ToList();
			whatever01.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever01.Attributes.Add("creation", new ValueTime("2012/10/18 07:03:00.000"));
			whatever01.Attributes.Add("cpu_usage", new ValueDouble(0.1));
			whatever01.Attributes.Add("num_cores", new ValueInt(7L));
			list = new Value[] {
				new ValueString("rewrite")
			}.ToList();
			whatever01.Attributes.Add("php_modules", new ValueList(list, AttributeTypePrimitive.String));
		
			ZMI whatever02 = new ZMI(pjwstk);
			pjwstk.AddSon(whatever02);
			whatever02.Attributes.Add("level", new ValueInt(2L));
			whatever02.Attributes.Add("name", new ValueString("whatever02"));
			whatever02.Attributes.Add("owner", new ValueString("/uw/whatever02"));
			whatever02.Attributes.Add("timestamp", new ValueTime("2012/11/09 21:13:00.000"));
			list = new Value[] {
				khaki31Contact, whatever01Contact
			}.ToList();
			whatever02.Attributes.Add("contacts", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever02.Attributes.Add("cardinality", new ValueInt(1L));
			list = new Value[] {
				whatever02Contact
			}.ToList();
			whatever02.Attributes.Add("members", new ValueSet(new HashSet<Value>(list), AttributeTypePrimitive.Contact));
			whatever02.Attributes.Add("creation", new ValueTime("2012/10/18 07:04:00.000"));
			whatever02.Attributes.Add("cpu_usage", new ValueDouble(0.4));
			whatever02.Attributes.Add("num_cores", new ValueInt(13L));
			list = new Value[] {
				new ValueString("odbc")
			}.ToList();
			whatever02.Attributes.Add("php_modules", new ValueList(list, AttributeTypePrimitive.String));
		
			return root;
		}
	}
}
