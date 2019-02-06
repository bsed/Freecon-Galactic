using Freecon.Core.Configs;

namespace Freecon.Server.Configs.Configs
{
    public class MasterServerConfig
    {
        /// <summary>
        /// Period with which the master server updates the redis DB to show that it is still online
        /// </summary>
        public int TimestampPeriodMS { get; set; }

        /// <summary>
        /// Timeout after which redis db timestamp expires if the server goes offline. Must be greater than TimestampPeriodMS
        /// </summary>
        public int TimestampTimeoutMS { get; set; }

        /// <summary>
        /// The timeout for a freshly spawned server, to give time for initialization 
        /// </summary>
        public int InitializationTimestampTimeoutMS { get; set; }

        /// <summary>
        /// Delay tims in ms before servers are rebalanced after a new slave connects, to allow multiple slaves to connect without spamming rebalances.
        /// </summary>
        public float RebalanceDelayMS { get; set; }

        public float SlaveHeartbeatPeriodMS { get; set; }

        /// <summary>
        /// Make the heartbeat persist just a little bit longer than the hearbeat period
        /// </summary>
        public float SlaveHeartbeatExpiryBuffer { get; set; }

        /// <summary>
        /// Time a slave has to initialize before sending a first ping. If a slave connects and no ping is received by this time, it is assumed that something went wrong.
        /// </summary>
        public float SlaveInitializationGracePeriod { get; set; }

        public NetworkConfigType Type { get; set; }

        public MasterServerConfig()
        {    
            Type = NetworkConfigType.Lidgren;

            TimestampPeriodMS = 1000;
            TimestampTimeoutMS = 4000;
            InitializationTimestampTimeoutMS = 5000;
            RebalanceDelayMS = 1000;
            SlaveHeartbeatPeriodMS = 1500;
            SlaveHeartbeatExpiryBuffer = SlaveHeartbeatPeriodMS + 500;
            SlaveInitializationGracePeriod = 8000;

        }

    }
}
