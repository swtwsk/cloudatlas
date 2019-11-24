﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using Shared.Model;
using Shared.Parsers;
using Shared.RPC;
using Attribute = Shared.Model.Attribute;

namespace Fetcher
{
    class Fetcher
    {
        private const string ZMI_TO_FETCH = "/uw/violet07";

        private static string _filename;
        private static string FileName => _filename ??= Guid.NewGuid().ToString();
        
        class Options
        {
            [Option("sHost", Default = "127.0.0.1", HelpText = "Server host name")]
            public string ServerHostName { get; set; }
			
            [Option("sPort", Default = 5000, HelpText = "Server port number")]
            public int ServerPortNumber { get; set; }
            
            [Option('i', "inifile", Required = true, HelpText = "Location of .ini file")]
            public string IniFileName { get; set; }
        }
        
        static void Main(string[] args)
        {
            var serverHostName = "";
            var serverPortNumber = 0;
            var collectionInterval = 1000;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    serverHostName = opts.ServerHostName;
                    serverPortNumber = opts.ServerPortNumber;
                    if (!TryParseCollectionInterval(opts.IniFileName, out collectionInterval))
                    {
                        Console.WriteLine($"COULDN'T FIND COLLECTION INTERVAL IN {opts.IniFileName} file");
                        Environment.Exit(1);
                    }
                })
                .WithNotParsed(errs =>
                {
                    foreach (var err in errs)
                        Console.WriteLine($"OPTIONS PARSE ERROR: {err}");
                    Environment.Exit(1);
                });
            
            var cts = new CancellationTokenSource();
            Console.WriteLine("Fetcher running. Press Enter to stop it...");
            var t = RunAsync(serverHostName, serverPortNumber, FileName, collectionInterval, cts.Token);
            Console.ReadLine();
            
            cts.Cancel();
            try
            {
                t.Wait();
            }
            catch (AggregateException)
            {
                // it is foreseen
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
            finally
            {
                File.Delete(FileName);
            }
        }

        private static bool TryParseCollectionInterval(string fileName, out int interval)
        {
            using (var file = File.OpenRead(fileName))
            {
                var stream = new StreamReader(file);
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    var split = line.Split('=', 2);
                    if (split.Length != 2)
                        continue;
                    var key = split[0];
                    var value = split[1];
                    if (key.Equals("collectionInterval") && int.TryParse(value, out interval))
                        return true;
                }
            }

            interval = -1;
            return false;
        }

        private static async Task RunAsync(string hostName, int portNumber, string fileName, int collectionInterval,
            CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }

                var channel = new Channel(hostName, portNumber, ChannelCredentials.Insecure);
                var invoker = new DefaultCallInvoker(channel);

                var _ = ProcessFile(invoker, ZMI_TO_FETCH, fileName)
                    .ContinueWith(async _ => await channel.ShutdownAsync(), token);
                await Task.Delay(collectionInterval, token);
            }
        }

        private static async Task ProcessFile(CallInvoker invoker, string pathName, string fileName)
        {
            var commandOut = $"./fetch.sh > {fileName}".Bash();
            
            await using var file = File.OpenRead(fileName);
            var stream = new StreamReader(file);
            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (line.StartsWith("/"))
                    continue;
                
                ZMIParser.TryParseAttributeLine(line, out var attribute, out var value);
                await SetAttribute(invoker, pathName, attribute, value);
            }
        }

        private static async Task SetAttribute(CallInvoker invoker, string pathName, Attribute attribute, Value value)
        {
            var attributeMsg = new AttributeMessage {PathName = pathName, Attribute = attribute, Value = value};
            using var call = invoker.AsyncUnaryCall(AgentMethods.SetAttribute, null, new CallOptions(), attributeMsg);
            var result = await call.ResponseAsync;
        }
        
        private static async Task SetContacts(CallInvoker invoker, ValueSet contacts)
        {
            using var call = invoker.AsyncUnaryCall(AgentMethods.SetContacts, null, new CallOptions(), contacts);
            var result = await call.ResponseAsync;
        }
    }
}