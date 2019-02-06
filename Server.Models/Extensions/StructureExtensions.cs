using System;
using Freecon.Core.Interfaces;

namespace Server.Models.Extensions
{
    public static class StructureExtensions
    {

        public static float DistanceTo(this IHasPosition s1, IHasPosition s2)
        {
            return (float)Math.Sqrt(Math.Pow(s1.PosX - s2.PosX, 2) + Math.Pow(s1.PosY - s2.PosY, 2));
        }

      
    
    }
}
