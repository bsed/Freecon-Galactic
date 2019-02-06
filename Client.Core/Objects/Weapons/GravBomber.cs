using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Freecon.Client.Objects.Weapons
{
    public class GravBomber : Laser
    {
        public GravBomber(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, holdingObj, slot)
        {
            Stats = new GravBomberStats();
        }


        protected override List<Vector2> GeneratePositions(float rotation)
        {
            List<Vector2> positions = new List<Vector2>(Stats.NumProjectiles);
            positions.Add(HoldingObj.Position);

            return positions;
        }


    }
}
