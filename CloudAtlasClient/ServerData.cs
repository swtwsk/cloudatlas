namespace CloudAtlasClient
{
    public interface IServerData
    {
        string HostName { get; }
        int PortNumber { get; }
    }

    public class ServerData : IServerData
    {
        public string HostName { get; }
        public int PortNumber { get; }

        public ServerData(string hostName, int portNumber)
        {
            HostName = hostName;
            PortNumber = portNumber;
        }
    }
}