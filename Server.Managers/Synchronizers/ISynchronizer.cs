namespace Server.Managers
{
    public interface ISynchronizer
    {
        void Start(float updatePeriod, int numThreads);

        void Stop();

    }
}
