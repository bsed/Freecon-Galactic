using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Models;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
namespace Freecon.Client.Objects.Weapons
{
    public class NaniteLauncher : Weapon
    {
        public NaniteLauncher(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager,  slot, holdingObj)
        {
            Stats = new NaniteLauncherStats();
        }

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            return new List<Vector2> { HoldingObj.Position };
        }

    }
    
}
