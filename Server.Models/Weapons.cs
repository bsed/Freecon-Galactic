using System;
using Freecon.Models.TypeEnums;
using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;
using Server.Models.Structures;
using MongoDB.Bson.Serialization.Attributes;
using Freecon.Core.Utils;
using Server.Interfaces;
using Freecon.Models;
using Freecon.Core.Interfaces;
using Freecon.Models.UI;
using Freecon.Core;

namespace Server.Models
{

    public abstract class Weapon:IHasUIData
    {
        public virtual string UIDisplayName { get { return Stats.WeaponType.ToString().SplitCamelCase(); } }

        [Key]
        public int Id { get; set; }

        [BsonIgnore]
        public float lastTimeStampAtFire;

        [UICollection]
        public WeaponStats Stats { get; set; }

        //Mostly a proof of concept for now. Can change to a list later for a weapon to have multiple debuffs
        [UIProperty]
        public DebuffTypes DebuffType { get; set; }
        [UIProperty]
        public int DebuffCount { get; set; }

        public Weapon() 
        {
            lastTimeStampAtFire = 0;
            DebuffType = DebuffTypes.None;
            DebuffCount = 1;
        }

        public virtual void Fire(IShip ship)
        {
            lastTimeStampAtFire = TimeKeeper.MsSinceInitialization;
            ship.ChangeEnergy(-Stats.EnergyCost);
        }

        public virtual bool CanFire(IShip ship)
        {
            return TimeKeeper.MsSinceInitialization - lastTimeStampAtFire >= Stats.FirePeriod &&
                   ship.CurrentEnergy >= Stats.EnergyCost && ship.GetArea().SecurityLevel != 255;

        }

        public virtual void Fire(IStructure s)
        {
            lastTimeStampAtFire = TimeKeeper.MsSinceInitialization;
        }

        public virtual bool CanFire(IStructure s)
        {
            return TimeKeeper.MsSinceInitialization - lastTimeStampAtFire >= Stats.FirePeriod;
        }
        
        public virtual void Update()
        {
        }
        
    }


    public class MissileLauncher : Weapon
    {
        public MissileLauncher(ProjectileTypes missileType)
        {
            Stats = new MissileLauncherStats(missileType);
        }

        public override bool CanFire(IShip ship)
        {

            return TimeKeeper.MsSinceInitialization - lastTimeStampAtFire >= Stats.FirePeriod &&
                   ship.CurrentEnergy >= Stats.EnergyCost && ship.GetArea().SecurityLevel != 255 &&
                   ship.Cargo.GetCargoAmount(((MissileLauncherStats)Stats).MissileType) > 0;
        }

        public override void Fire(IShip ship)
        {
            lastTimeStampAtFire = TimeKeeper.MsSinceInitialization;
            ship.ChangeEnergy(-Stats.EnergyCost);
        }


        public void SetMissileType(ProjectileTypes missileType)
        {
            if (Stats.ProjectileType == missileType)
            {
                return;
            }
            Stats = new MissileLauncherStats(missileType);
        }
        
    }

    public class Laser : Weapon
    {

        public Laser()
        {
            Stats = new LaserWeaponStats();
        }
        
    }

    public class AltLaser : Weapon
    {

        public AltLaser()
        {
            Stats = new AltLaserStats();
        }

    }

    public class HurrDurr : Weapon
    {
 
        public HurrDurr()
        {
            Stats = new HurrDurrStats();
        }

    }

    public class PlasmaCannon : Weapon
    {

        public PlasmaCannon()
        {
            Stats = new PlasmaCannonStats();
        }

    }

    public class BC_Laser : Weapon
    {

        public BC_Laser()
        {
            Stats = new BC_LaserStats();
        }
    }

    public class GravBomber : Weapon
    {
       
        public GravBomber()
        {
            Stats = new GravBomberStats();
        }
    }

    public class LaserWave : Weapon
    {
       
        public LaserWave()
        {
            Stats = new LaserWaveStats();
        }
    }
    
    public class NullWeapon : Weapon
    {

        public NullWeapon()
        {
            Stats = new NoneWeaponStats();
        }
    }

    /// <summary>
    /// "Weapon" which "fires" the an explosion when a mine detonates. Not for ships.
    /// </summary>
    public class MineWeapon : Weapon
    {

        public MineWeapon()
        {
            Stats = new MineWeaponStats();
        }
   
    }
}