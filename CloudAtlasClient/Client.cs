using System;
using CommandLine;
using Nancy.Hosting.Self;

namespace CloudAtlasClient
{
    class Client
    {
        class Options
        {
            [Option("sHost", Default = "127.0.0.1", HelpText = "Server host name")]
            public string ServerHostName { get; set; }
			
            [Option("sPort", Default = 5000, HelpText = "Server port number")]
            public int ServerPortNumber { get; set; }
            
            [Option("qHost", Default = "127.0.0.1", HelpText = "Query signer host name")]
            public string SignerHostName { get; set; }
            
            [Option("qPort", Default = 6666, HelpText = "Query signer port number")]
            public int SignerPortNumber { get; set; }
            
            [Option('h', "host", Default = "127.0.0.1", HelpText = "Client host name")]
            public string HostName { get; set; }
            
            [Option('p', "port", Default = 8888, HelpText = "Client port number")]
            public int PortNumber { get; set; }
        }
        
        static void Main(string[] args)
        {
            IServerData serverData = null;
            Uri apiUri = null;
            
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    serverData = new ServerData(opts.ServerHostName, opts.ServerPortNumber, opts.SignerHostName,
                        opts.SignerPortNumber); 
                    apiUri = new Uri($"http://{opts.HostName}:{opts.PortNumber}");
                })
                .WithNotParsed(errs =>
                {
                    foreach (var err in errs)
                        Console.WriteLine($"OPTIONS PARSE ERROR: {err}");
                    Environment.Exit(1);
                });

            using var host = new NancyHost(new Bootstrapper(serverData), apiUri);
            host.Start();
            Console.WriteLine($"Client running on {apiUri}. Press Enter to stop it...");
            Console.ReadLine();
        }
    }
}
