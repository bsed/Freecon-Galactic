using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;
namespace Freecon.Client.Extensions
{
    public static class FarseerExtensions
    {
        public static Vector2 ToSimUnits(this Vector2 vector)
        {
            return ConvertUnits.ToSimUnits(vector);
        }
        public static Vector2 ToDisplayUnits(this Vector2 vector)
        {
            return ConvertUnits.ToDisplayUnits(vector);
        }
        public static double ToSimUnits(this double vector)
        {
            return vector / 100f;
        }
        public static double ToDisplayUnits(this double vector)
        {
            return vector * 100f;
        }

        public static double ConvertToCartesian(Vector2 vel)
        {
            var incomingArcTan = (float)Math.Atan2(-vel.Y, vel.X) + MathHelper.ToRadians(90);

            // Get the arch tangent: y/x, then convert from Farseer
            // Converting from farseer's retarded coordinate system
            return incomingArcTan * Math.PI / MathHelper.ToRadians(180);
        }
    }
}
