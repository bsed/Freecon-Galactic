using System;
using Freecon.Core.Interfaces;
using Freecon.Core.Utils;

namespace Server.Models.Mathematics
{
    public static class SpatialOperations
    {
        /// <summary>
        /// Gets a point in a disk defined by innerRadius and outerRadius radii and centered on the passed x and y values.
        /// </summary>
        public static void GetRandomPointInRadius(ref float x, ref float y, float innerRadius, float outerRadius)
        {
            float length = innerRadius + (outerRadius - innerRadius) * (float)Rand.Random.NextDouble();

            float angle = (float) (2*Math.PI*Rand.Random.NextDouble());

            x += length * (float)Math.Cos(angle);
            y += length * (float)Math.Sin(angle);           
                     
        }

        /// <summary>
        /// Sets position a point in a disk defined by innerRadius and outerRadius radii and centered on obj's current position.
        /// </summary>
        public static void SetRandomPointInRadius(this IHasPosition obj, float innerRadius, float outerRadius)
        {
            float length = innerRadius + (outerRadius - innerRadius) * (float)Rand.Random.NextDouble();

            float angle = (float) (2*Math.PI*Rand.Random.NextDouble());

            obj.PosX += length * (float)Math.Cos(angle);
            obj.PosY += length * (float)Math.Sin(angle);

        }

        public static float GetDistance(this IHasPosition s, IHasPosition c)
        {
            return (float)Math.Sqrt((s.PosX*-c.PosX) + (s.PosY - c.PosY)); 
        }

    }
}