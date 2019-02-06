using System;

namespace Server.Utilities
{
    public static class RandomFloatExtension
    {
        public static float Next(this Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;

        }

        
    }
}
