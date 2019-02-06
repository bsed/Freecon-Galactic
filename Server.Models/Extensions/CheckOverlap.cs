using System;

namespace Server.Models.Extensions
{
    //Todo: rewrite with some kind of interfaces exposing size and position
    public static class StructureChecks
    {        

        /// <summary>
        /// Returns true if objects overlap, false otherwise.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool CheckOverlap(this Planet p, float sizex, float sizey, float posx, float posy)
        {
            //Approximating the structure as a circle
            float radii = (float)Math.Sqrt(sizex * sizex + sizey * sizey) / 2f + p.AreaSize;

            float distance = (float)(Math.Sqrt(Math.Pow(posx - p.PosX, 2) + Math.Pow(posy - p.PosY, 2)));

            return distance < radii;

        }


    }
}
