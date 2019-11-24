using System;
using System.IO;
using Shared.Model;
using Shared.Parsers;

namespace Interpreter
{
	public class Program {
		private static ZMI _root;

		private const string DEFAULT_CONFIG_FILE = "zmis.txt";
	
		public static void Main(string[] args)
		{
			var pathToConfig = args.Length > 0 ? args[0] : DEFAULT_CONFIG_FILE;
			if (!File.Exists(pathToConfig))
			{
				Console.Error.WriteLine($"Could not find config file in path: {pathToConfig}");
				System.Environment.Exit(1);
			}

			using (var file = File.OpenRead(pathToConfig))
			{
				if (!ZMIParser.TryParseZMI(new StreamReader(file), ref _root))
				{
					Console.Error.WriteLine($"Could not parse ZMI from file: {pathToConfig}");
					System.Environment.Exit(1);
				}
			}
			
			var scanner = Console.In;
			string line;
		
			while ((line = scanner.ReadLine()) != null)
			{
				Interpreter.ExecuteQueries(_root, line, true);
			}
		}
	}
}
