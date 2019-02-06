using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Freecon.Client.Managers;
using Freecon.Models;
using Server.Managers;

namespace Freecon.Client.Objects.Weapons
{
    public class PlasmaCannon : ChargableWeapon, IChargable
    {
        public PlasmaCannon(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, slot, holdingObj)
        {
            if (!(holdingObj is Ship))
                ConsoleManager.WriteLine("WARNING: Chargable weapons are not yet implemented for non-ship objects! Crashes imminent!", ConsoleMessageType.Warning);

            Stats = new PlasmaCannonStats();

        }

        public void Charge(double elapsedTimeMS)
        {
            double chargeAmount = elapsedTimeMS / 1000d * _chargeRate;
            if (_currentCharge < _maxCharge)
            {
                _currentCharge += chargeAmount;
                HoldingObj.ChangeEnergy(-(int)(chargeAmount * _energyPerCharge));

            }
        }

        public void ResetCharge()
        {
            _currentCharge = 0;

        }

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            return new List<Vector2> { OffsetBullet(HoldingObj.Position, rotation, 0, .18f) };
        }

        public override void Update(IGameTimeService gameTime)
        {

            base.Update(gameTime);
            //Update charging text here, if necessary, or draw charging effect, or whatever
            //Probably easiest to use a sprite sheet with a premade plasma graphic until we replace mercury

        }
        

      



    }
}
