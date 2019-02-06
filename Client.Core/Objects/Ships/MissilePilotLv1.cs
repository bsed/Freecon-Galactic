using System;
using Freecon.Client.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Client.Objects.Projectiles;
using Core.Interfaces;
using Microsoft.Xna.Framework;

namespace Freecon.Client.Objects.Pilots
{
    public class MissilePilotLv1 : Pilot
    {
        protected IMissile _missile;
        protected float _targetRange = 10;


        public MissilePilotLv1(IMissile missile, float targetRange)
        {
            _missile = missile;
        }

        public override void Simulate(IGameTimeService gameTime)
        {
            

            
            if (CurrentTarget == null || !CurrentTarget.IsBodyValid)
            {
                _findTarget(_targetRange);
            }
            else
            {
                AIHelper.TurnTowardPosition(_missile.Rotation, _missile.CurrentTurnRate, _missile.Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, 1f);
            }  
                 


            _missile.ThrustForward();
        }


        protected void _findTarget(float engagementRange)
        {

            float smallestDistance = float.MaxValue;
            //This targeting scheme is just temporary, may need to send an update to the server for a set target event, similar to NPC logic
            foreach (var kvp in _missile.PotentialTargets)
            {
                if (kvp.Value == _missile || kvp.Value.Id == _missile.FiringObj.Id)
                {
                    Console.WriteLine("Missile was targetting itself, removing from potential targets.");//Pretty sure this has been resolved
                    _missile.PotentialTargets.Remove(((IProjectile)_missile).Id);
                    _missile.PotentialTargets.Remove(_missile.FiringObj.Id);
                    return;
                }

                if (kvp.Value.IsBodyValid && Vector2.Distance(kvp.Value.Position, _missile.Position) < smallestDistance && Vector2.Distance(kvp.Value.Position, _missile.Position) <= engagementRange)
                {
                    smallestDistance = Vector2.Distance(kvp.Value.Position, _missile.Position);
                    CurrentTarget = kvp.Value;
                }
            }

        }

    }
    public class MissilePilotLv2 : MissilePilotLv1
    {
        public MissilePilotLv2(IMissile missile, float targetRange)
            : base(missile, targetRange)
        {

        }

        public override void Simulate(IGameTimeService gameTime)
        {


            if (CurrentTarget == null || !CurrentTarget.IsBodyValid)
            {
                _findTarget(_targetRange);
                //_missile.ThrustForward();
            }
            else
            {


                //Rotate the missile so that it is thrusting and pointing in the correct direction
                //AIHelper.TurnTowardPosition(ref newRotation, _missile.CurrentTurnRate, _missile.Position, CurrentTarget.Position, (float)gameTime.ElapsedMilliseconds, 0.001f);

                //Get corrective acceleration
                //Lower knav (last argument) to reduce effectiveness of correction
                Vector2 correctiveAccel = AIHelper.PNav(_missile.Position, _missile.LinearVelocity, CurrentTarget.Position, CurrentTarget.LinearVelocity, 6f);
                _missile.CorrectVelocity(correctiveAccel);
                //_missile.ThrustForward();

            }

            _missile.ThrustForward();

        }


    }
}