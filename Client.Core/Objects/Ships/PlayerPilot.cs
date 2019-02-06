using System;
using Core.Interfaces;
using Core.Models.Enums;
using Microsoft.Xna.Framework;

namespace Freecon.Client.Objects.Pilots
{
    public class PlayerPilot : Pilot
    {
        public bool boosting = false;
        public double timeToRespawn = 0; // Milliseconds, sent from server, right now only used for mothership battle

        public PlayerPilot(Ship ship)
        {
            this.Ship = ship;
            PilotType = PilotType.Player;
        }

        public override void Update(IGameTimeService gameTime)
        {
            if (!IsAlive)
            {
                return;
            }
        }

        /// <summary>
        /// Applies thrust in the direction opposite to linear velocity to slow the ship to a stop
        /// </summary>
        public void StopShip(IGameTimeService gameTime)
        {
            if (Ship.LinearVelocity.Length() == 0)
            {
                Ship.Thrusting = false;
                return;
            }
            else if (Ship.LinearVelocity.Length() < .05)
            {
                Ship.LinearVelocity = new Vector2(0, 0);
                Ship.Thrusting = false;
                return;
            }


            if (!TurnTowardPosition(Ship.Position - Ship.LinearVelocity, gameTime, .01f))
            {
                Ship.Thrust(ThrustTypes.Forward);
                Ship.Thrusting = true;
            }
        }

        /// <summary>
        /// Returns true if ship rotated, returns false if ship did not need to rotate or is now pointing at the passed position
        /// If the angle difference less than tolerance, the ship's angle is set and false is returned
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool TurnTowardPosition(Vector2 position, IGameTimeService gameTime, float tolerance)
        {
            //Turn to opposite of linear velocity vector
            float angleToTurn = getAngleToPosition(position) - Ship.Rotation;

            if (Math.Abs(angleToTurn) > 0)
            {
                if (Math.Abs(angleToTurn) < .01)//Set ship to the correct angle, to avoid constantly rotating ship, because rotations occur over discrete timesteps
                {
                    Ship.Rotation = Ship.Rotation + angleToTurn;
                    return false;
                }
                else
                    if (angleToTurn > 0)
                    {
                        Ship.Rotation += Ship.ShipStats.TurnRate * (float)(gameTime.ElapsedMilliseconds) / 1000;
                        return true;
                    }
                    else
                    {
                        Ship.Rotation -= Ship.ShipStats.TurnRate * (float)(gameTime.ElapsedMilliseconds) / 1000;
                        return true;
                    }
            }

            return false;
        }
        protected float getAngleToPosition(Vector2 p)
        {
            Vector2 r = new Vector2(p.X - Ship.Position.X, p.Y - Ship.Position.Y);
            return (-(float)Math.Atan2(r.X, r.Y) + (float)Math.PI);

        }
    }
}