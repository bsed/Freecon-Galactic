using Core.Interfaces;
using Freecon.Core.Utils;

namespace Client.Bot
{
    public class BotnetGametimeService : IGameTimeService
    {
        public float TotalMilliseconds { get { return TimeKeeper.MsSinceInitialization; } }

        public float ElapsedMilliseconds { get; set; }

    }
}
