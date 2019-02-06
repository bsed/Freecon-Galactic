using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Microsoft.Xna.Framework;
using Freecon.Client.Managers;
using Freecon.Models;
using Freecon.Client.Core.Utils;

namespace Freecon.Client.Objects.Weapons
{
    class HurrDurr:Laser
    {
        public HurrDurr(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, holdingObj, slot)
        {
            Stats = new HurrDurrStats();   
        }

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            return Utilities.CreateArcPositions(Stats.NumProjectiles, 360, 1, 0, HoldingObj.Position);
        }


    }
}
