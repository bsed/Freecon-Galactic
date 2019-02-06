using Freecon.Core.Configs;

namespace Freecon.Server.Configs
{
    public class DevelopNetworkConfig : INetworkConfig
    {
        public NetworkConfigType Type { get { return NetworkConfigType.Lidgren; } }

        public string Address { get { return "73.136.102.14"; } }

        public int Port { get { return 28000; } }

        public string ServerName { get { return "Freecon Galactic"; } }
    }

    public class TestNetworkConfig : INetworkConfig
    {
        public NetworkConfigType Type { get { return NetworkConfigType.Lidgren; } }

        public string Address { get { return "127.0.0.1"; } }

        public int Port { get { return 28000; } }

        public string ServerName { get { return "Freecon Galactic"; } }
    }
}
