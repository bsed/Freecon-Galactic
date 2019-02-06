using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Freecon.Models.TypeEnums;
using Freecon.Client.Managers;
using Freecon.Core;
using Freecon.Models;

namespace Freecon.Client.Objects.Weapons
{
    public class MissileLauncher : Weapon
    {

        public MissileLauncher(ProjectileManager projectileManager, ProjectileTypes missileType, Ship holdingObj, byte slot)
            : base(projectileManager, slot, holdingObj)
        {
            Stats = new MissileLauncherStats(missileType);
        }

        public void SetMissileType(ProjectileTypes missileType)
        {
            Stats = new MissileLauncherStats(missileType);
        }

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            return new List<Vector2> { HoldingObj.Position };
        }

        public override bool CanFire()
        {            
            return HoldingObj.GetCurrentEnergy() >= Stats.EnergyCost &&//Check if there is enough energy 
                   timeSinceLastShot >= Stats.FirePeriod &&//Fire period check 
                   (!WaitingForFireResponse || TimeOfWaitStart > waitTimeOut) && ((Ship)HoldingObj).Cargo.GetCargoAmount(((MissileLauncherStats)Stats).MissileType) >= Stats.NumProjectiles;//Try removing missile only if everything else is OK. 
            //TODO: Reimplement ICanFire to enable non-ships to use missile launchers...
        }

     

        public override bool Fire_LocalOrigin(float rotation, byte charge, bool changeEnergy)
        {
            ((Ship)HoldingObj).Cargo.RemoveStatelessCargo(((MissileLauncherStats)Stats).MissileType, 1);
            return base.Fire_LocalOrigin(rotation, charge, changeEnergy);
        }
    }
}
