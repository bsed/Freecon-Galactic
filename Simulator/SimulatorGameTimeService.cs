using Core.Interfaces;
using Freecon.Core.Utils;

namespace Simulator
{
    public class SimulatorGameTimeService:IGameTimeService
    {
        public float TotalMilliseconds { get { return TimeKeeper.MsSinceInitialization; } }

        public float ElapsedMilliseconds { get; set; }

    }
}
