namespace CloudAtlasClient
{
    public interface IServerData
    {
        string HostName { get; set; }
        int PortNumber { get; set; }
        string SignerHostName { get; }
        int SignerPortNumber { get; }
    }

    public class ServerData : IServerData
    {
        public string HostName { get; set; }
        public int PortNumber { get; set; }
        public string SignerHostName { get; }
        public int SignerPortNumber { get; }

        public ServerData(string hostName, int portNumber, string signerHostName, int signerPortNumber)
        {
            HostName = hostName;
            PortNumber = portNumber;
            SignerHostName = signerHostName;
            SignerPortNumber = signerPortNumber;
        }
    }
}