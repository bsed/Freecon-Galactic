using System;
using System.Diagnostics;

namespace Freecon.Core.Utils
{
    public static class TimeKeeper
    {
        static public Stopwatch Stopwatch;

        static long _initTime;//Time when this object is initialized

        static bool _isInit;

        /// <summary>
        /// Gets a UTC timestamp, zeroed to the initialization time of this server. Attempt to overcome low precision of DateTime.UtcNow(). There will be some error between stamps from different server instances, because DateTime.UtcNow() has low (~16-30ms) precision
        /// </summary>
        public static long GetUTCStamp { get { return (Stopwatch.ElapsedTicks - _initTime) / TimeSpan.TicksPerMillisecond; } }

        public static int GetUnixTime()
        {
            return (int) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        static TimeKeeper()
        {
            if(!_isInit)
                Initialize();
        }

        static public void Initialize()
        {
            if (!_isInit)
            {
                Stopwatch = new Stopwatch();
                Stopwatch.Start();
                _initTime = DateTime.UtcNow.Ticks;
                _isInit = true;
            }
        }

        /// <summary>
        /// Resolution down to ticks
        /// </summary>
        static public float MsSinceInitialization{ get{ return (float)Stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000; } }

        static public float TicksSinceInitialization { get { return Stopwatch.ElapsedTicks; } }
    }
}
