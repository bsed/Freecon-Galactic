using Freecon.Models.TypeEnums;
using Server.Interfaces;
using Server.Models.Structures;

namespace Server.Models.Extensions
{
    public static class ProjectileDamage
    {
        public static float DamageAmount(this IShip s, ProjectileTypes type, byte pctCharge)
        {
            //Calculate damage based on modules and whatever else WARNING: needs to be implemented properly
            switch (type)
            {
                case ProjectileTypes.Laser:
                    return 500;
                case ProjectileTypes.BC_Laser:
                    return 900;
                case ProjectileTypes.LaserWave:
                    return 150;
                case ProjectileTypes.PlasmaCannon:
                    return (2000 * pctCharge);
                case ProjectileTypes.AmbassadorMissile:
                    return (20000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.HellHoundMissile:
                    return (30000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType1:
                    return (20000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType2:
                    return (20000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType3:
                    return (20000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType4:
                    return (20000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MineSplash:
                    return 900*pctCharge;
                default:
                    return 100;
            }

        }

        public static float DamageAmount(this Turret t, ProjectileTypes type, byte pctCharge)
        {
            //Calculate damage based on modules and whatever else WARNING: needs to be implemented properly
            switch (type)
            {
                case ProjectileTypes.Laser:
                    return 100;
                case ProjectileTypes.BC_Laser:
                    return 150;
                case ProjectileTypes.LaserWave:
                    return 50;
                case ProjectileTypes.PlasmaCannon:
                    return (1000 * pctCharge);
                case ProjectileTypes.AmbassadorMissile:
                    return (1000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.HellHoundMissile:
                    return (500 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType1:
                    return (1000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType2:
                    return (1000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType3:
                    return (1000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType4:
                    return (1000 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MineSplash:
                    return 900 * pctCharge;
                default:
                    return 100;
            }

        }

        public static float EnergyAmount(this IShip s, ProjectileTypes type, byte pctCharge)
        {
            switch (type)
            {
                case ProjectileTypes.Laser:
                    return 50;
                case ProjectileTypes.BC_Laser:
                    return 150;
                case ProjectileTypes.LaserWave:
                    return 200;
                case ProjectileTypes.PlasmaCannon:
                    return (500 * pctCharge);
                case ProjectileTypes.AmbassadorMissile:
                    return (200 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.HellHoundMissile:
                    return (300 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType1:
                    return (200 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType2:
                    return (200 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType3:
                    return (200 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MissileType4:
                    return (200 * pctCharge);//Splash represented by pctCharge
                case ProjectileTypes.MineSplash:
                    return 900 * pctCharge;
                default:
                    return 50;
            }


        }

    }
}
