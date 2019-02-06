using System;
using System.Collections;
using Freecon.Client.Core.Coroutines;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Mathematics;
using Core.Interfaces;
using Core.Models.Enums;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;

namespace Freecon.Client.Objects.Pilots
{
    public class NPCPilot : Pilot
    {
        private LogicStates MacroState = LogicStates.Roaming;
        private LogicStates _lastState = LogicStates.Stopped;//Used to resume state for temporary states, e.g. moving to warp for warp command

        protected byte MicroState = 0;

        private float _engagementRange = 4;//Minimum range of engagement during target selection

        private Vector2 _destination;//Used for goto command

        protected IGameTimeService _lastGameTime;
        protected ICoroutineManager _coroutineManager;

        public NPCPilot(Ship ship, CollisionDataObject userdata)
        {
            PilotType = PilotType.NPC;
            Ship = ship;

            _coroutineManager = new CoroutineManager();

            ship.SetPilotData(userdata, false);
        }

        private bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            //Don't collide with other network/player ships
            return ((CollisionDataObject)fixtureB.Body.UserData).BodyType != BodyTypes.NetworkShip && ((CollisionDataObject)fixtureB.Body.UserData).BodyType != BodyTypes.PlayerShip;
        }

        public override void Update(IGameTimeService gameTime)
        {
            _lastGameTime = gameTime;

            if (Ship.IsLocalSim)
            {
                Simulate(gameTime);
            }

            _coroutineManager.Update(gameTime);
        }

        /// <summary>
        /// Move to destination, ignoring targets along the way.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable MoveToPosition(Vector2 destination)
        {
            var randomness = _rand.Next(0, 10) / 10f;

            yield return new TurnTowards(this, randomness, () => _destination);

            while (true)
            {
                

                var result = AIHelper.TurnTowardPosition(
                    Ship.Rotation,
                    Ship.ShipStats.TurnRate,
                    Ship.Position,
                    _destination,
                    _lastGameTime.ElapsedMilliseconds,
                    .2f
                );

                Ship.Rotation = result.Rotation;

                if (!result.Rotated)
                {
                    Ship.Thrust(ThrustTypes.Forward);
                    Ship.Thrusting = true;
                }
                yield return new Wait();
            }

            // Ship is at destination, go back to whatever it was doing
            MacroState = _lastState == LogicStates.Roaming ? _lastState : LogicStates.HoldingPosition;

            Ship.Thrusting = false;
        }

        public override void Simulate(IGameTimeService gameTime)
        {
            if (!Ship.IsBodyValid || !Ship.IsAlive)
                return;

            Ship.Thrusting = false;

            if (MacroState == LogicStates.AttackingAll)
            {
                //Find nearest target in system, attack, repeat, do nothing if no targets are available
                #region AttackingAll
                var vecToTarget = new Vector2();

                if (CurrentTarget != null)
                {
                    if (CurrentTarget.IsBodyValid)
                        vecToTarget = getVectorToPosition(CurrentTarget.Position);
                    else
                    {
                        CurrentTarget = null;
                        MicroState = 0;
                    }

                }
                else
                    MicroState = 0;




                #region Logic

                if (Ship.CurrentHealth <= 0)
                    return;

                switch (MicroState)
                {
                    case 0: //Do nothing

                        _findTarget(10000);//"Infinite" range


                        if (CurrentTarget != null)
                        {
                            MicroState = 1;
                        }
                        break;

                    case 1: //Turn to face target
                    {
                            
                        var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate, Ship.Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, .175f);

                        Ship.Rotation = result.Rotation;

                        if (!result.Rotated)
                        {
                            MicroState = 2;
                        }

                        break;
                    }
                    case 2: //Fly toward target
                    {
                        var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate, Ship.Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, .1f);
                        Ship.Rotation = result.Rotation;

                        if (Math.Abs((Ship.Position - CurrentTarget.Position).Length()) >= 3)
                        {
                            Ship.Thrust(ThrustTypes.Forward);
                            Ship.Thrusting = true; //Need a better way to do this
                        }
                        if (vecToTarget.Length() <= 3)
                        {
                            MicroState = 3; //Start strafing
                        }
                        break;
                    }

                    case 3: //Strafe target
                    {

                        var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate, Ship.Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, .1f);
                        Ship.Rotation = result.Rotation;

                        if (CurrentTarget.TargetType == TargetTypes.Moving)
                        {
                            Ship.Thrust(ThrustTypes.Forward);
                            Ship.Thrusting = true;
                        }
                        Ship.TryFireWeapon(gameTime,1);

                        if (vecToTarget.Length() >= 5)
                        {
                            MicroState = 1; //Stop strafing, turn to face player again
                            Ship.Thrusting = false;
                        }
                        break;
                    }
                    case 4:

                        break;
                }

                #endregion
                #endregion
            }
            else if (MacroState == LogicStates.HoldingPosition)
            {        
                //Hold the ship's position
                StopShip(gameTime);
            }
            else if (MacroState == LogicStates.MovingToPosition)
            {
                //Move to _destination, ignoring targets along the way
                #region MovingToPosition

                //Tolerance is randomized so all ships don't move to the same location
                //Should probably move tolerance outside of this loop so that it isn't set on every iteration
                if ((Ship.Position - _destination).Length() < 3f && Ship.LinearVelocity.Length() > .1f)
                    StopShip(gameTime);//Ship is within range of the destination, start slowing down
                else if((Ship.Position - _destination).Length() < (float)_rand.Next(0, 10) / 10f && Ship.LinearVelocity.Length() <= .1)
                {
                    //Ship is at destination, go back to whatever it was doing
                    if (_lastState == LogicStates.Roaming)
                        MacroState = _lastState;
                    else
                        MacroState = LogicStates.HoldingPosition;
                    Ship.Thrusting = false;
                    
                }
                else
                {
                    var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate, Ship.Position, _destination, (float)gameTime.ElapsedMilliseconds, .2f);

                    Ship.Rotation = result.Rotation;

                    if (!result.Rotated)
                    {
                        Ship.Thrust(ThrustTypes.Forward);
                        Ship.Thrusting = true;
                    }
                }
                #endregion
            }
            else if (MacroState == LogicStates.AttackingTarget)
            {
                //Pursue and attack target until it is killed or warps
                #region AttackingTarget

                if (CurrentTarget == null || !CurrentTarget.IsBodyValid)
                {
                    CurrentTarget = null;//Set to null if target is no longer valid
                    if (_lastState != MacroState)//To avoid getting stuck in this state
                        MacroState = _lastState;
                    else
                        MacroState = LogicStates.Roaming;

                    return;
                }
                switch (MicroState)
                {

                    case 1: //Turn to face target
                    {
                        var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate,
                            Ship.Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, .2f);

                        Ship.Rotation = result.Rotation;

                        if (!result.Rotated)
                        {
                            MicroState = 2;
                        }

                        break;
                    }

                    case 2: //Fly toward target
                    {
                        var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate,
                            Ship.Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, .2f);

                        Ship.Rotation = result.Rotation;

                        if ((Ship.Position - CurrentTarget.Position).Length() >= 3)
                        {
                            Ship.Thrust(ThrustTypes.Forward);
                            Ship.Thrusting = true;
                        }
                        if ((Ship.Position - CurrentTarget.Position).Length() <= 3)
                        {
                            MicroState = 3; //Start strafing
                        }
                        break;
                    }
                    case 3: //Strafe target
                    {
                        var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate, Ship.Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, .2f);

                        Ship.Rotation = result.Rotation;

                        if (CurrentTarget.TargetType == TargetTypes.Moving)
                        {
                            Ship.Thrust(ThrustTypes.Forward);
                            Ship.Thrusting = true;
                        }
                        Ship.TryFireWeapon(gameTime, 1);

                        if ((Ship.Position - CurrentTarget.Position).Length() >= 5)
                        {
                            MicroState = 1; //Stop strafing, turn to face target again
                            Ship.Thrusting = false;
                        }
                        break;
                    }
                }
                #endregion

            }
            else if (MacroState == LogicStates.Roaming)
            {

                #region Roaming

                //Really hacky stupid way to temporarily generate random positions
                _destination = new Vector2((_rand.Next(-15, 15)), _rand.Next(-15, 15));
                if (_rand.Next(0, 2) == 0)//Avoids destinations in the sun
                    _destination.X *= -1;
                if (_rand.Next(0, 2) == 0)
                    _destination.Y *= -1;
                //Console.WriteLine("Setting Course for " + _destination);


                _lastState = LogicStates.Roaming;
                MacroState = LogicStates.MovingToPosition;

                #endregion
            }
            else if (MacroState == LogicStates.AttackingToPosition)
            {
                if(CurrentTarget == null)
                    _findTarget(_engagementRange);
                
                if (CurrentTarget == null)//No targets available within range
                {
                    
                    if ((Ship.Position - _destination).Length() < 2f && Ship.LinearVelocity.Length() > .01)
                        StopShip(gameTime);//Ship is within range of the destination, start slowing down
                    if ((Ship.Position - _destination).Length() < (float)_rand.Next(0, 10) / 10f && Ship.LinearVelocity.Length() <= .01)
                    {
                        //Ship is at destination, hold position aggressively
                        MacroState = LogicStates.HoldPosAggressively;
                        Ship.Thrusting = false;

                    }
                    else 
                    {
                        var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate, Ship.Position, _destination, gameTime.ElapsedMilliseconds, .2f);
                        Ship.Rotation = result.Rotation;

                        if (!result.Rotated)
                        {
                            Ship.Thrust(ThrustTypes.Forward);
                            Ship.Thrusting = true;
                        }
                    }

                }
                else
                {
                    MacroState = LogicStates.AttackingTarget;
                    MicroState = 1;
                    _lastState = LogicStates.AttackingToPosition;

                }

            }
            else if (MacroState == LogicStates.HoldPosAggressively)
            {
                if (CurrentTarget == null)
                    _findTarget(_engagementRange);

                if (CurrentTarget == null)
                    StopShip(gameTime);//Hold the ship's position
                else
                {
                    MacroState = LogicStates.AttackingTarget;
                    _lastState = LogicStates.HoldPosAggressively;
                }
            }

        }

        /// <summary>
        /// Applies thrust in the direction opposite to linear velocity to slow the ship to a stop
        /// </summary>
        private void StopShip(IGameTimeService gameTime)
        {
            if (Ship.LinearVelocity.Length() == 0)
            {
                Ship.Thrusting = false;
                return;
            }

            if (Ship.LinearVelocity.Length() < .01)
            {
                Ship.LinearVelocity = new Vector2(0, 0);
                Ship.Thrusting = false;
                return;
            }


            var result = AIHelper.TurnTowardPosition(Ship.Rotation, Ship.ShipStats.TurnRate, Ship.Position, Ship.Position - Ship.LinearVelocity * gameTime.ElapsedMilliseconds, gameTime.ElapsedMilliseconds, .01f);
            Ship.Rotation = result.Rotation;
            if (!result.Rotated)
            {
                Ship.Thrust(ThrustTypes.Forward);
                Ship.Thrusting = true;
            }

        }
        /// <summary>
        /// Allows the pilot to logically choose a new target
        /// </summary>
        /// <param name="firingObject"></param>
        public override void HitByTargetable(ITargetable firingObject)
        {

            // Randomly selects a target for now, 1/5 probability of choosing the firingObject
            if (CurrentTarget == null && firingObject.IsBodyValid)
            {
                CurrentTarget = firingObject;
                return;
            }

            if (_rand.Next(5) == 1)
            {
                CurrentTarget = firingObject;
                if (MacroState == LogicStates.Roaming || MacroState == LogicStates.MovingToPosition)
                    MacroState = LogicStates.AttackingAll;
            }
        }    

        #region SelectorCommands

        public override void HoldPosition()
        {
            if(Ship.IsLocalSim)
                MacroState = LogicStates.HoldingPosition;

        }

        public override void GoToPosition(Vector2 destination)
        {
            _destination = destination;
            MacroState = LogicStates.MovingToPosition;
        }

        public override void Stop()
        {
            MacroState = LogicStates.Stopped;

        }

        public override void AttackTarget(ITargetable target)
        {
            MacroState = LogicStates.AttackingTarget;
            MicroState = 1;
            CurrentTarget = target;
        }

        public override void AttackToPosition(Vector2 destination)
        {
            _destination = destination;
            MacroState = LogicStates.AttackingToPosition;
            _lastState = LogicStates.MovingToPosition;
            CurrentTarget = null;
        }

        #endregion

        #region Update Helper Functions (like getting angles and stuff)

        /// <summary>
        /// Returns a separation vector from the NPC to the passed targetShip
        /// In farseer's coordinate system (positive Y is downscreen)
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        private Vector2 getVectorToPosition(Vector2 targetPosition)
        {
            var vecToTarget = new Vector2();
            vecToTarget.X = targetPosition.X - Ship.Position.X;
            vecToTarget.Y = targetPosition.Y - Ship.Position.Y;
            return vecToTarget;
        }

          

        protected void _findTarget(float engagementRange)
        {


            float smallestDistance = float.MaxValue;
            //This targeting scheme is just temporary, may need to send an update to the server for a set target event, similar to NPC logic
            foreach (var kvp in Ship.PotentialTargets)
            {
                if (kvp.Value == Ship)
                {
                    Console.WriteLine("NPC was targetting itself, removing from potential targets.");//Pretty sure this has been resolved
                    Ship.PotentialTargets.Remove(Ship.Id);
                    return;
                }

                if (kvp.Value.IsBodyValid && Vector2.Distance(kvp.Value.Position, Ship.Position) < smallestDistance && Vector2.Distance(kvp.Value.Position, Ship.Position) <= engagementRange)
                {
                    smallestDistance = Vector2.Distance(kvp.Value.Position, Ship.Position);
                    CurrentTarget = kvp.Value;

                }
            }

        }

        #endregion
    }
}