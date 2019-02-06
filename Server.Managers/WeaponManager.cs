using Freecon.Models.TypeEnums;
using Server.Models;

namespace Server.Managers
{
    /// <summary>
    /// I can't remember why I thought this was a good idea.
    /// </summary>
    public class WeaponManager
    {
        /// <summary>
        /// Instantiates a new instance of the given weapon type
        /// </summary>
        /// <param name="weaponType"></param>
        /// <param name="missileType">Used inly if weaponType is MissileLauncher</param>
        /// <returns></returns>
        static public Weapon GetNewWeapon(WeaponTypes weaponType, ProjectileTypes missileType = ProjectileTypes.AmbassadorMissile)
        {

            switch (weaponType)
            {
                case WeaponTypes.Laser:
                    return new Laser();
                case WeaponTypes.AltLaser:
                    return new AltLaser();
                case WeaponTypes.HurrDurr:
                    return new HurrDurr();
                case WeaponTypes.PlasmaCannon:
                    return new PlasmaCannon();
                case WeaponTypes.BC_Laser:
                    return new BC_Laser();
                case WeaponTypes.LaserWave:
                    return new LaserWave();
                case WeaponTypes.MissileLauncher:
                    return new MissileLauncher(missileType);
                case WeaponTypes.GravBomber:
                    return new GravBomber();
                case WeaponTypes.MineWeapon:
                    return new MineWeapon();



                case WeaponTypes.None:
                    return new NullWeapon();
                default:
                    ConsoleManager.WriteLine("Error: " + weaponType.ToString() + " flyweight not yet implemented in WeaponManager.cs.", ConsoleMessageType.Error);
                    return new NullWeapon();




            }

          
        }

    }
}