using Freecon.Core.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Server.Configs
{
    public class TestNetworkConfig : INetworkConfig
    {
        public NetworkConfigType Type { get { return NetworkConfigType.Test; } }

        public string Address { get { return "127.0.0.1"; } }

        public int Port { get { return 28000; } }

        public string ServerName { get { return "ALLYOURBASE"; } }
    }
}
