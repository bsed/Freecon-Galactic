using System;
using System.Collections.Generic;
using Freecon.Client.Interfaces;

using Microsoft.Xna.Framework;
using Freecon.Models.TypeEnums;
using Freecon.Client.Managers;
using Freecon.Models;

namespace Freecon.Client.Objects.Weapons
{
    public class Laser : Weapon
    {

        public Laser(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, slot, holdingObj)
        {
            Stats = new LaserWeaponStats();
        }

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            List<Vector2> projPos = new List<Vector2>();

            switch (((Ship)HoldingObj).ShipStats.ShipType)
            {
                case ShipTypes.Barge:
                    projPos.Add(OffsetBullet(HoldingObj.Position, rotation, 0.27f, 0.31f));
                    projPos.Add(OffsetBullet(HoldingObj.Position, rotation, -0.27f, 0.31f));
                    break;

                case ShipTypes.FrogShip:
                case ShipTypes.Penguin:
                case ShipTypes.Dread:
                case ShipTypes.XBoxController:
                case ShipTypes.Reaper:
                    projPos.Add(OffsetBullet(HoldingObj.Position, rotation, 0.18f, 0.18f));
                    projPos.Add(OffsetBullet(HoldingObj.Position, rotation, -0.18f, 0.18f));
                    break;

                case ShipTypes.BattleCruiser:
                    projPos.Add(OffsetBullet(HoldingObj.Position, rotation, 0.03f, 0.25f));
                    projPos.Add(OffsetBullet(HoldingObj.Position, rotation, -0.03f, 0.25f));
                    break;

                default:
                    Console.WriteLine("Warning: this ship has not yet been implemented in Laser.GeneratePositions().");
                    for (int i = 0; i < Stats.NumProjectiles; i++)
                        projPos.Add((new Vector2(i, i)));
                    break;
            }

            return projPos;
        }


    }
}
