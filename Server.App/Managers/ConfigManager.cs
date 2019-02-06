using Freecon.Core.Configs;
using Freecon.Server.Configs;
using System;
using System.Collections.Generic;

namespace Freecon.Server.App
{
    /// <summary>
    /// Todo: Integrate this with Master server to make known port/IP configs.
    /// </summary>
    public class ConfigManager
    {
        public List<INetworkConfig> NetworkConfigs { get; private set; }

        public ConfigManager(FreeconServerEnvironment environment)
        {
            NetworkConfigs = new List<INetworkConfig>();

            switch (environment)
            {
                case FreeconServerEnvironment.Development:
                case FreeconServerEnvironment.Production:
                    NetworkConfigs.Add(new DevelopNetworkConfig());
                    break;
                case FreeconServerEnvironment.Test:
                    NetworkConfigs.Add(new TestNetworkConfig());
                    break;
                default:
                    throw new Exception("Wrong environment mother fucker");
            }
        }
    }
}
