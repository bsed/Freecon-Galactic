namespace Freecon.Core.Configs
{
    public class NetworkConfig : INetworkConfig
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public string ServerName { get; set; }

        public NetworkConfigType Type { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        public NetworkConfig()
        {
            

        }

    }
}
