using Freecon.Client.Core.Utils;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using FarseerPhysics;
using Freecon.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Freecon.Client.Objects.Weapons
{
    public class LaserWave : Weapon
    {
        public LaserWave(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, slot, holdingObj)
        {
            Stats = new LaserWaveStats();
        }
        

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            Vector2 firePos = OffsetBullet(HoldingObj.Position, rotation, 0, ConvertUnits.ToSimUnits(HoldingObj.BodyHeight / 2));
            return Utilities.CreateArcPositions(Stats.NumProjectiles, 75, ConvertUnits.ToSimUnits(HoldingObj.BodyHeight / 2 - 2), HoldingObj.Rotation, firePos);

        }


        protected override List<float> GenerateRotations(float rotation)
        {
            Vector2 firePos = OffsetBullet(HoldingObj.Position, rotation, 0, ConvertUnits.ToSimUnits(HoldingObj.BodyHeight / 2));
            return Utilities.CreateArcRotations(Stats.NumProjectiles, 55, ConvertUnits.ToSimUnits(HoldingObj.BodyHeight / 2 - 2), HoldingObj.Rotation, firePos);

        }





    }
}
