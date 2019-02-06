using Freecon.Models.TypeEnums;

namespace Freecon.Core
{
    public class Validation
    {
        public static bool IsMissileType(ProjectileTypes projectileType)
        {
            switch (projectileType)
            {
                case ProjectileTypes.AmbassadorMissile:
                case ProjectileTypes.HellHoundMissile:
                case ProjectileTypes.MissileType1:
                case ProjectileTypes.MissileType2:
                case ProjectileTypes.MissileType3:
                case ProjectileTypes.MissileType4:
                    return true;
                default:
                    return false;
            }

        }

    }
}
