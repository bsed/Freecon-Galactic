using Freecon.Core.Configs;

namespace Freecon.Server.Configs
{
    public class ConnectionManagerConfig
    {
        public NetworkConfig MyConfig;

        public CoreNetworkConfig CoreConfig;

        public ConnectionManagerConfig(CoreNetworkConfig coreNetworkConfig)
        {
            MyConfig = new NetworkConfig();

            CoreConfig = coreNetworkConfig;

            MyConfig.Address = "73.136.102.14";

            MyConfig.Port = 28000;

            MyConfig.ServerName = CoreConfig.ServerName;

            MyConfig.Type = NetworkConfigType.Lidgren;

            MyConfig.ReceiveBufferSize = 1000000;

            MyConfig.SendBufferSize = 1000000;

        }


    }
}
