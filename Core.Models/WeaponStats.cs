using System;
using Freecon.Core;
using Freecon.Core.Interfaces;
using Freecon.Models.TypeEnums;
using Freecon.Models.UI;

namespace Freecon.Models
{
    public abstract class WeaponStats:IHasUIData
    {
        public virtual string UIDisplayName { get { return WeaponType.ToString().SplitCamelCase(); } }

        [UIProperty(Units="TW")]
        public float EnergyCost { get; set; } // Energy cost to fire weapon

        [UIProperty(Units="s")]
        public float FirePeriod { get; set; } // Minimum time between shots, in milliseconds

        public ProjectileTypes ProjectileType { get; protected set; }

        [UIProperty()]
        public byte NumProjectiles { get; set; } // Number of projectiles fired on fire command. Tells the server how many IDs to generate

        [UIProperty()]
        public WeaponTypes WeaponType { get; protected set; } // Used for server to keep track of the type of weapon on a ship.

        [UIProperty(Units="kg")]
        public float Weight { get; set; } // Weight of the weapon, in case we make weapons affect ship handling

    }

    public abstract class ChargeableWeaponStats : WeaponStats
    {
        [UIProperty()]
        public float MaxCharge { get; set; }

        [UIProperty()]
        public float ChargeRate { get; set; }//Charge per s

        [UIProperty()]
        public float EnergyPerCharge { get; set; }//Can take up to _energyPerCharge * _maxCharge energy during a charge    

    }

    public class LaserWeaponStats : WeaponStats
    {
        public LaserWeaponStats()
        {
            Weight = 100;
            EnergyCost = 10;
            FirePeriod = 100; //milliseconds
            NumProjectiles = 2;
            WeaponType = WeaponTypes.Laser;
            ProjectileType = ProjectileTypes.Laser;
        }

    }

    public class AltLaserStats : WeaponStats
    {
        public AltLaserStats()
        {
            Weight = 100;
            EnergyCost = 50;
            FirePeriod = 50;
            NumProjectiles = 1;
            ProjectileType = ProjectileTypes.Laser;
            WeaponType = WeaponTypes.AltLaser;
        }        

    }
    public class TurretAltLaserStats : WeaponStats
    {
        public TurretAltLaserStats()
        {
            Weight = 100;
            EnergyCost = 50;
            FirePeriod = 200;
            NumProjectiles = 1;
            ProjectileType = ProjectileTypes.Laser;
            WeaponType = WeaponTypes.AltLaser;
        }

    }

    public class NoneWeaponStats : WeaponStats
    {
        public NoneWeaponStats()
        {
            Weight = 0;
            EnergyCost = 0;
            FirePeriod = float.MaxValue;
            WeaponType = WeaponTypes.None;
        }

    }

    public class BC_LaserStats : WeaponStats
    {
        public BC_LaserStats()
        {
            Weight = 300;
            EnergyCost = 800;
            FirePeriod = 400; //milliseconds
            NumProjectiles = 3;
            WeaponType = WeaponTypes.BC_Laser;
            ProjectileType = ProjectileTypes.BC_Laser;
        }

    }

    public class LaserWaveStats : WeaponStats
    {
        
        public LaserWaveStats()
        {
            Weight = 400;
            EnergyCost = 100;
            FirePeriod = 100; //milliseconds
            NumProjectiles = 12;
            WeaponType = WeaponTypes.LaserWave;
            ProjectileType = ProjectileTypes.LaserWave;
        }
    }

    public class MineWeaponStats : WeaponStats
    {

        public MineWeaponStats()
        {
            Weight = 0;
            EnergyCost = 0;
            FirePeriod = 0; //milliseconds
            NumProjectiles = 1;
            WeaponType = WeaponTypes.MineWeapon;
            ProjectileType = ProjectileTypes.MineSplash;
        }
    }
    public class HurrDurrStats : WeaponStats
    {
        public HurrDurrStats()
        {
            Weight = 100;
            EnergyCost = 10;
            FirePeriod = 20; //milliseconds
            NumProjectiles = 50;
            WeaponType = WeaponTypes.HurrDurr;
            ProjectileType = ProjectileTypes.Laser;

        }
    }
    
    public class MissileLauncherStats:WeaponStats
    {
        public StatelessCargoTypes MissileType; 

        public MissileLauncherStats(ProjectileTypes missileType)
        {
            Weight = 0;

            if (!Validation.IsMissileType(missileType))
            {
                throw new InvalidOperationException("Non missile type passed to MissileLauncherStats constructor");
            }

            MissileType = (StatelessCargoTypes)Enum.Parse(typeof(StatelessCargoTypes), missileType.ToString());
            ProjectileType = missileType;
            NumProjectiles = 1;
            FirePeriod = 100f;
            EnergyCost = 100f;
            WeaponType = WeaponTypes.MissileLauncher;
        
        }

    }

    public class NaniteLauncherStats : WeaponStats
    {
        public NaniteLauncherStats()
        {
            Weight = 100;
            EnergyCost = 10;
            FirePeriod = 100; //milliseconds
            NumProjectiles = 1;
            WeaponType = WeaponTypes.NaniteLauncher;
            ProjectileType = ProjectileTypes.NaniteLauncher;
        }
    }

    public class PlasmaCannonStats : WeaponStats
    {
        public PlasmaCannonStats()
        {
            Weight = 100;
            EnergyCost = 150;
            FirePeriod = 250; //milliseconds
            NumProjectiles = 1;          
            WeaponType = WeaponTypes.PlasmaCannon;
            ProjectileType = ProjectileTypes.PlasmaCannon;
        }
    }

    public class GravBomberStats : WeaponStats
    {

        public GravBomberStats()
        {
            Weight = 1000;
            EnergyCost = 100;
            FirePeriod = 300; //milliseconds
            NumProjectiles = 1;
            WeaponType = WeaponTypes.GravBomber;
            ProjectileType = ProjectileTypes.GravityBomb;

        }

    }

}
