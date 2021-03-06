﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using CloudAtlasAgent.Modules.GossipStrategies;
using CommandLine;
using Shared;
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
			[Option('k', "key", Required = true, HelpText = "Path to public key")]
			public string PublicKeyPath { get; set; }
			
			[Option('c', "config", Default = null, HelpText = "ZMI config file path")]
			public string ConfigFile { get; set; }
			
			[Option('i', "inifile", Default = null, HelpText = "Location of .ini file")]
			public string IniFileName { get; set; }
			
			[Option('n', "name", Required = true, HelpText = "Name of ZMI node")]
			public string ZmiName { get; set; }
			
			[Option('h', "host", Default = "127.0.0.1", HelpText = "Receiver host IP")]
			public string ReceiverHost { get; set; }
			
			[Option('p', "port", Default = 5000, HelpText = "Receiver port")]
			public int ReceiverPort { get; set; }

			[Option("rpc", Default = "127.0.0.1", HelpText = "RPC host IP")]
			public string RpcHost { get; set; }
			
			[Option("rpcport", Default = 5001, HelpText = "RPC port number")]
			public int RpcPort { get; set; }
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
			var zmiName = string.Empty;
			ZMI fatherZmi = null;
			RSA rsa = null;
			var receiverHost = string.Empty;
			var receiverPort = 0;
			var rpcHost = string.Empty;
			var rpcPort = 0;

			IDictionary<string, string> configuration = new Dictionary<string, string>();

			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(opts =>
				{
					zmiName = opts.ZmiName.Trim(' ');
					if (string.IsNullOrEmpty(opts.ConfigFile) || !TryParseConfig(opts.ConfigFile, out fatherZmi))
						fatherZmi = ZMI.FromPathName(zmiName);

					receiverHost = opts.ReceiverHost;
					receiverPort = opts.ReceiverPort;
					rpcHost = opts.RpcHost;
					rpcPort = opts.RpcPort;
					
					rsa = RSAFactory.FromPublicKey(opts.PublicKeyPath);
					
					if (string.IsNullOrEmpty(opts.IniFileName))
						return;

					using var file = File.OpenRead(opts.IniFileName);
					using var stream = new StreamReader(file);
					configuration = INIParser.ParseIni(stream);
				})
				.WithNotParsed(errs =>
				{
					foreach (var err in errs)
						Console.WriteLine($"OPTIONS PARSE ERROR: {err}");
					Environment.Exit(1);
				});

			var creationTimestamp = new ValueTime(DateTimeOffset.Now);
			
			fatherZmi.ApplyForEach(zmi => zmi.Attributes.AddOrChange("update", creationTimestamp));
			if (!fatherZmi.TrySearch(zmiName, out var myZmi))
			{
				Console.WriteLine($"Could not find node {zmiName} in ZMIs");
				Environment.Exit(1);
			}
			myZmi.Attributes.AddOrChange("timestamp", creationTimestamp);
			myZmi.Attributes.AddOrChange("contacts",
				new ValueSet(
					new HashSet<Value>(new[]
						{new ValueContact(myZmi.PathName, IPAddress.Parse(receiverHost), receiverPort)}),
					AttributeTypePrimitive.Contact));
			myZmi.Attributes.AddOrChange("isSingleton", new ValueBoolean(true));

			var manager = ManagerFromIni(receiverHost, receiverPort, rpcHost, rpcPort, configuration, rsa, myZmi);
			
			Console.WriteLine("Press ENTER to exit...");
			Console.ReadLine();
			Console.WriteLine("End");
			manager.Dispose();
		}

		private static ModulesManager ManagerFromIni(string receiverHost, int receiverPort, string rpcHost, int rpcPort,
			IDictionary<string, string> configuration, RSA rsa, ZMI zmi)
		{
			if (!configuration.TryGetInt("queryInterval", out var queryInterval))
				queryInterval = 5;
			if (!configuration.TryGetInt("gossipInterval", out var gossipInterval))
				gossipInterval = 5;
			if (!configuration.TryGetInt("purgeInterval", out var purgeInterval))
				purgeInterval = 60;
			if (!configuration.TryGetInt("receiverTimeout", out var receiverTimeout))
				receiverTimeout = 3000;
			if (!configuration.TryGetInt("retryInterval", out var retryInterval))
				retryInterval = 2;
			if (!configuration.TryGetInt("maxRetriesCount", out var maxRetriesCount))
				maxRetriesCount = 5;
			if (!configuration.TryGetInt("maxPacketSize", out var maxPacketSize))
				maxPacketSize = 2000;
			if (!configuration.TryGetValue("gossipStrategy", out var gossipStrategyStr) ||
			    !TryGetGossipStrategy(gossipStrategyStr, out var gossipStrategy))
				gossipStrategy = new RandomGossipStrategy();
			if (!configuration.TryGetValue("loggerLevel", out var loggerLevelStr) ||
			    !TryGetLoggerLevel(loggerLevelStr, out var loggerLevel))
				loggerLevel = LoggerLevel.All;
			if (!configuration.TryGetValue("loggerVerbosity", out var loggerVerbosityStr) ||
			    !TryGetLoggerVerbosity(loggerVerbosityStr, out var loggerVerbosity))
				loggerVerbosity = LoggerVerbosity.WithFileName;

			receiverHost = receiverHost.Trim(' ');
			rpcHost = rpcHost.Trim(' ');
			
			Logger.LoggerLevel = loggerLevel;
			Logger.LoggerVerbosity = loggerVerbosity;

			return new ModulesManager(maxPacketSize, receiverHost, receiverPort, receiverTimeout, rpcHost, rpcPort,
				queryInterval, purgeInterval, rsa, gossipStrategy, gossipInterval, retryInterval, maxRetriesCount, zmi);
		}

		private static bool TryGetGossipStrategy(string strategyName, out IGossipStrategy gossipStrategy)
		{
			switch (strategyName)
			{
				case "random":
					gossipStrategy = new RandomGossipStrategy();
					return true;
				case "randomExp":
					gossipStrategy = new RandomExponentialGossipStrategy();
					return true;
				case "roundRobin":
					gossipStrategy = new RoundRobinGossipStrategy();
					return true;
				case "roundRobinExp":
					gossipStrategy = new RoundRobinExponentialGossipStrategy();
					return true;
			}

			gossipStrategy = null;
			return false;
		}

		private static bool TryGetLoggerLevel(string loggerName, out LoggerLevel loggerLevel)
		{
			switch (loggerName)
			{
				case "nothing":
				case "Nothing":
					loggerLevel = LoggerLevel.Nothing;
					return true;
				case "exception":
				case "Exception":
					loggerLevel = LoggerLevel.Exception;
					return true;
				case "error":
				case "Error":
					loggerLevel = LoggerLevel.Error;
					return true;
				case "warning":
				case "Warning":
					loggerLevel = LoggerLevel.Warning;
					return true;
				case "all":
				case "All":
					loggerLevel = LoggerLevel.All;
					return true;
			}

			loggerLevel = LoggerLevel.All;
			return false;
		}

		private static bool TryGetLoggerVerbosity(string verbosityName, out LoggerVerbosity loggerVerbosity)
		{
			switch (verbosityName.ToLower())
			{
				case "withfilepath":
					loggerVerbosity = LoggerVerbosity.WithFilePath;
					return true;
				case "withfilename":
					loggerVerbosity = LoggerVerbosity.WithFileName;
					return true;
				case "withoutfilepath":
					loggerVerbosity = LoggerVerbosity.WithoutFilePath;
					return true;
			}

			loggerVerbosity = LoggerVerbosity.WithFileName;
			return false;
		}
	}
}
