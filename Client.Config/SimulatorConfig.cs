namespace Freecon.Client.Config
{
    public class SimulatorConfig
    {
        public string RedisChannelPrefix = "Sim_";

        /// <summary>
        /// Update interval for states without active clients
        /// </summary>
        public float EmptyGameStateUpdateIntervalMS = 200;

        /// <summary>
        /// Update interval for states with active clients
        /// </summary>
        public float OccupiedGameStateUpdateIntervalMS = 17;


        public float MainManagerUpdateIntervalMS = 17;
    }
}
