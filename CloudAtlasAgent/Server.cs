using System;
using System.IO;
using System.Security.Cryptography;
using CommandLine;
using Grpc.Core;
using Shared.Logger;
using Shared.Model;
using Shared.Parsers;
using Shared.RSA;

namespace CloudAtlasAgent
{
	public class Server
	{
		class Options
		{
			[Option('h', "host", Default = "127.0.0.1", HelpText = "Server host name")]
			public string HostName { get; set; }
			
			[Option('p', "port", Default = 5000, HelpText = "Server port number")]
			public int PortNumber { get; set; }
			
			[Option('k', "key", HelpText = "Path to public key", SetName = "keys", Required = true)]
			public string PublicKeyPath { get; set; }
			
			[Option('c', "config", Default = "zmis.txt", HelpText = "ZMI config file path")]
			public string ConfigFile { get; set; }
			
			[Option('n', "name", Required = true, HelpText = "Name of ZMI node")]
			public string ZmiName { get; set; }
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
			var zmiName = string.Empty;
			var receiverHost = string.Empty;
			var receiverPort = 0;

			ZMI fatherZmi = null;
			RSA rsa = null;

			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(opts =>
				{
					if (!TryParseConfig(opts.ConfigFile, out fatherZmi))
						Environment.Exit(1);
					serverPort = new ServerPort(opts.HostName.Trim(' '), opts.PortNumber, ServerCredentials.Insecure);
					zmiName = opts.ZmiName.Trim(' ');

					// TODO: CHANGE IT!
					receiverHost = serverPort.Host;
					receiverPort = serverPort.Port + 1;

					rsa = RSAFactory.FromPublicKey(opts.PublicKeyPath);
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
			
			fatherZmi.ApplyForEach(zmi => zmi.Attributes.AddOrChange("update", creationTimestamp));
			if (!fatherZmi.TrySearch(zmiName, out var myZmi))
			{
				Console.WriteLine($"Could not find node {zmiName} in ZMIs");
				Environment.Exit(1);
			}
			myZmi.Attributes.AddOrChange("timestamp", creationTimestamp);

			var manager = new ModulesManager(2000, receiverHost, receiverPort, 3000, serverPort.Host,
				serverPort.Port, 5, 20, rsa, 7, 2, 5, myZmi);
			
			Console.WriteLine($"Agent started on {receiverHost}:{receiverPort}\nRPC started on {serverPort.Host}:{serverPort.Port}");
			Console.WriteLine("Press ENTER to exit...");
			Console.ReadLine();
			Console.WriteLine("End");
			manager.Dispose();
		}
	}
}
