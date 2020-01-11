namespace CloudAtlasClient
{
    public interface IServerData
    {
        string HostName { get; }
        int PortNumber { get; }
        string SignerHostName { get; }
        int SignerPortNumber { get; }
    }

    public class ServerData : IServerData
    {
        public string HostName { get; }
        public int PortNumber { get; }
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