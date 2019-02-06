namespace Freecon.Server.Configs
{
    public class ServerConfig
    {
        public float StructureUpdatePeriod = 10000; // Time in milliseconds between building updates
        public float ObjectUpdatePeriod = 20;
        public float ConsoleRedrawPeriodMs = 1000;
        public float DBSyncPeriod = 5000;
        public float MasterServerManagerUpdatePeriod = 1000;

        public float CargoSynchronizerUpdatePeriod = 30;
        public int CargoSynchronizerNumThreads = 4;

        public float TradeSynchronizerUpdatePeriod = 30;
        public int TradeSynchronizerNumThreads = 4;

        //Minimum time allowed between consecutive warps, to prevent spam
        public float MinWarpPeriod = 100;


        public float MessageProcessingDeadlockTimeout = 10000;

    }
}
