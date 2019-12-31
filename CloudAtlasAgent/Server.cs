using System;
using System.IO;
using System.Linq;
using CommandLine;
using Grpc.Core;
using Shared.Logger;
using Shared.Model;
using Shared.Parsers;

namespace CloudAtlasAgent
{
	public class Server
	{
		private static ZMI _zmi;

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
//					_log = opts.Log;
				})
				.WithNotParsed(errs =>
				{
					foreach (var err in errs)
						Console.WriteLine($"OPTIONS PARSE ERROR: {err}");
					Environment.Exit(1);
				});

			Logger.LoggerLevel = LoggerLevel.All;
			Logger.LoggerVerbosity = LoggerVerbosity.WithFileName;

			var creationTimestamp = new ValueTime(DateTimeOffset.Now);
			var manageableZmi = _zmi;
			while (manageableZmi.Sons.Any())
			{
//				manageableZmi.Attributes.AddOrChange("timestamp", creationTimestamp);
				manageableZmi.Attributes.AddOrChange("freshness", creationTimestamp);  // TODO: For now, remove it after
				manageableZmi = manageableZmi.Sons.First();
			}
			manageableZmi.Attributes.AddOrChange("timestamp", creationTimestamp);
			manageableZmi.Attributes.AddOrChange("freshness", creationTimestamp);

			using var manager = new ModulesManager(2000, serverPort.Host, serverPort.Port + 1, 3000, serverPort.Host,
				serverPort.Port, 7, 2, 5, manageableZmi);

			manager.Start();
			Console.ReadLine();
			Console.WriteLine("End");
		}
	}
}
