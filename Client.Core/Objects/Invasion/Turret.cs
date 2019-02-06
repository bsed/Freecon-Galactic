using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Interfaces;
using System.Collections.Generic;
using FarseerPhysics.Factories;
using Freecon.Client.Objects.Weapons;
using FarseerPhysics.Dynamics;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Extensions;
using FarseerPhysics.Dynamics.Contacts;
using Freecon.Core.Networking.Models.Messages;

namespace Freecon.Client.Objects.Structures
{
    public class Turret
        : Structure, ICollidable, ITargetable, ISimulatable,
          ITargeter, ITeamable, ISelectable, ICommandable, ICanFire, ISynchronousUpdate
    {
        private readonly Random r;
        
        protected static readonly float rotationSpeed = .50f * (float)Math.PI;
        protected static readonly float turretRange = 20;
        protected static readonly float AimTolerance = .0174f;

        protected Texture2D BaseTexture;

        public TurretTypes TurretType;

        protected MessageService_ToServer _messageService;//Temporary


        protected LogicStates logicState = 0;
        protected float angleToTarget;

        protected ProjectileManager _projectileManager;
        protected ClientShipManager _clientShipManager;

        private bool _isValid { get; set; }
        
        new public bool IsBodyValid { get 
        { return _isValid && !_body.IsDisposed; } set { _isValid = value; } }

        public int Energy { get { return 10000; } set{} }

        public bool IsSelected { get; set; }

        public float ATanAngle;
        protected float DelayTime = 5000, LoadWait = 500;
        protected float OldTime;

        protected float _lastTargetCheckTime;//To enable closest object targeting while preventing target search spam

        protected float SpeedX, SpeedY;

        public float BodyHeight { get { return 86 / 2; } }//Need to set this properly

        public Turret(
            MessageService_ToServer messageService,
            ProjectileManager projectileManager,
            Texture2D baseTexture,
            Texture2D headTexture,
            ClientShipManager clientShipManager,
            SpriteBatch spriteBatch,
            World w,
            Vector2 position,
            StructureTypes type,
            float health,
            int ID,
            int seed,
            TurretTypes turretType,
            HashSet<int> teams)
            : base(spriteBatch, headTexture, position.X, position.Y, StructureTypes.LaserTurret, health, ID, teams)
        {
            _messageService = messageService;
            Debugging.AddStack.Push(this.ToString());
            _body = BodyFactory.CreateRectangle(w, .5f, .5f, 1, position);
            _body.IsStatic = true;
            _body.UserData = new StructureBodyDataObject(BodyTypes.Turret, this);
            _body.OnCollision += OnCollision;


            r = new Random(seed); // Sets a seed so that turrets fire at different times.

            _projectileManager = projectileManager;
            _clientShipManager = clientShipManager;

            Weapon = new TurretAltLaser(projectileManager, this, 1);
            Console.WriteLine(ID);

            Texture = headTexture;
            BaseTexture = baseTexture;

            PotentialTargets = new Dictionary<int, ITargetable>();
         
            Teams = teams;

            IsBodyValid = true;

            TurretType = turretType;

        }

        public override void Update(IGameTimeService gameTime)
        {
            if (IsLocalSim)
            {
                Simulate(gameTime);
                return;
            }




            if (IsDead) return;
            Weapon.Update(gameTime);
            DelayTime += gameTime.ElapsedMilliseconds; // Used for Loading.

           
            #region Logic

                //0 = No target(s) in range, laser at rest
                //1 = Target(s) on planet, find one
                //2 = Turn head to aim or Fire if within aim tolerance
                //3 = Fire 

                switch (logicState)
                    //Just FYI in case you're confused, a locally simulated turret doesn't run this code, it runs .Simulate() instead.
                {
                    case LogicStates.Resting:
                        if (PotentialTargets.Count > 0)
                            logicState = LogicStates.SearchingForTarget;

                        break;

                    case LogicStates.SearchingForTarget:
                        FindTarget();
                        if (CurrentTarget != null)
                            logicState = LogicStates.TurningTowardTarget;
                        break;

                    case LogicStates.TurningTowardTarget:
                        if (CurrentTarget == null)
                        {
                            logicState = LogicStates.Resting;
                            break;
                        }
                        else if (CurrentTarget != null && !CurrentTarget.IsBodyValid)
                        {
                            PotentialTargets.Remove(CurrentTarget.Id);
                            CurrentTarget = null;
                            logicState = LogicStates.Resting;
                            break;
                        }
                        else if (gameTime.TotalMilliseconds - _lastTargetCheckTime > 200)
                            //Retargets closest item 5 times per second
                            //Retargeting is expensive, if turrets lag this needs to be rethought
                        {
                            _lastTargetCheckTime = (float) gameTime.TotalMilliseconds;
                            CurrentTarget = null;
                            logicState = LogicStates.Resting;
                            break;
                        }
                        else if (Vector2.Distance(CurrentTarget.Position, Position) > turretRange)
                        {
                            CurrentTarget = null;
                            logicState = LogicStates.Resting;
                            break;
                        }


                        Rotation = AIHelper.TurnTowardPosition(
                            Rotation, rotationSpeed,
                            Position, CurrentTarget.Position,
                            gameTime.ElapsedMilliseconds, AimTolerance
                        ).Rotation;



                        break;
                }

                #endregion
            
            OldTime += gameTime.ElapsedMilliseconds; // Updates Time since Last Action
        }

        public void ChangeEnergy(int amount)
        {
            //Only here to satisfy ICanFire requirements
        }

        public int GetCurrentEnergy()
        {
            return 999999999;//Only here to satisfy ICanFire requirements
        }

        public void SetCurrentEnergy(int value)
        {
            //Only here to satisfy ICanFire requirements
        }

        public override void Draw(Camera2D camera)
        {
            Color c = Color.White;
            if (IsSelected)
                c = Color.Green;

            _spriteBatch.Draw(BaseTexture, ConvertUnits.ToDisplayUnits(Position), null, c, 0,
                             new Vector2(Texture.Width / 2, Texture.Height / 2), 1f, SpriteEffects.None, RenderDepths.TurretHead);

            _spriteBatch.Draw(Texture, ConvertUnits.ToDisplayUnits(Position), null, c, _body.Rotation,
                             new Vector2(Texture.Width / 2, Texture.Height / 2), 1f, SpriteEffects.None, RenderDepths.TurretBase);
        }

        protected void FindTarget()
        {
            float smallestDistance = float.MaxValue;
            //This targeting scheme is just temporary, may need to send an update to the server for a set target event, similar to NPC logic
            foreach (var kvp in PotentialTargets)
            {
                if (kvp.Value == this)
                {
                    Console.WriteLine("Turret was targetting itself, removing from potential targets.");//Haven't been able to track down where a turret is added to its own list of potential targets
                    PotentialTargets.Remove(Id);
                    return;
                }

                if (kvp.Value.IsBodyValid && Vector2.Distance(kvp.Value.Position, Position) < smallestDistance && Vector2.Distance(kvp.Value.Position, Position) < turretRange)
                {
                    smallestDistance = Vector2.Distance(kvp.Value.Position, Position);
                    CurrentTarget = kvp.Value;

                }
            }

        }
        public void SetTarget(ITargetable ship)
        {
            CurrentTarget = (ITargetable)ship;
        }

        bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body.UserData is ShipBodyDataObject)
            {
                var s = fixtureB.Body.UserData as ShipBodyDataObject;
                if (!s.Ship.OnSameTeam(this))
                    return true;//Collide with enemy turrets

                if (s.Ship.EnterMode && s.Ship.Cargo.CheckCargoSpace(StatefulCargoTypes.LaserTurret, 1))
                {
                    _messageService.SendObjectPickupRequest(s.ID, Id, PickupableTypes.Turret);

                }
            }





            return false;
        }

        public override void Simulate(IGameTimeService gameTime)
        {
            if (!IsBodyValid)
                return;
            Weapon.Update(gameTime);
            DelayTime += gameTime.ElapsedMilliseconds; // Used for Loading.

            #region Logic
            

            if(logicState != LogicStates.Resting && (CurrentTarget == null || !CurrentTarget.IsBodyValid))
            {
                CurrentTarget = null;
                logicState = LogicStates.SearchingForTarget;

            }
            switch (logicState)
            {
                case LogicStates.Resting:
                    if (PotentialTargets.Count > 0)
                        logicState = LogicStates.SearchingForTarget;

                    break;

                case LogicStates.SearchingForTarget:
                    FindTarget();
                    if (CurrentTarget != null)
                        logicState = LogicStates.TurningTowardTarget;
                    break;

                case LogicStates.TurningTowardTarget:
                    if (!CurrentTarget.IsBodyValid)
                    {
                        PotentialTargets.Remove(CurrentTarget.Id);
                        CurrentTarget = null;
                        logicState = LogicStates.Resting;
                        break;
                    }

                    if (gameTime.TotalMilliseconds - _lastTargetCheckTime > 200)//Retargets closest item 5 times per second
                    //Retargeting is expensive, if turrets lag this needs to be rethought
                    {
                        _lastTargetCheckTime = (float)gameTime.TotalMilliseconds;
                        CurrentTarget = null;
                        logicState = LogicStates.Resting;
                        break;
                    }


                    if (Vector2.Distance(CurrentTarget.Position, Position) > turretRange)
                    {
                        CurrentTarget = null;
                        logicState = LogicStates.Resting;
                        break;

                    }


                    var result = AIHelper.TurnTowardPosition(Rotation, rotationSpeed, Position, CurrentTarget.Position, gameTime.ElapsedMilliseconds, AimTolerance);

                    Rotation = result.Rotation;

                    if (!result.Rotated)
                    {
                        if (Weapon.CanFire())
                        {
                           
                            Weapon.TimeOfWaitStart = gameTime.TotalMilliseconds;
                            Weapon.Fire_LocalOrigin(Rotation, 0, true);
                            Weapon.WaitingForFireResponse = true;
                           
                        }
                    }
                    break;
            }

            #endregion

            OldTime += gameTime.ElapsedMilliseconds; // Updates Time since Last Action
        }

        #region ICommandable Methods

        public virtual void HoldPosition()
        {

        }
        public virtual void GoToPosition(Vector2 destination)
        {

        }
        public virtual void AttackToPosition(Vector2 destination)
        {

        }
        public virtual void Stop()
        {

        }
        public virtual void AttackTarget(ITargetable target)
        {
            CurrentTarget = target;

        }

        #endregion

    }


    public enum LogicStates : byte
    {
        //0 = No target(s) in range, laser at rest
        Resting,

        //1 = Target(s) on planet, find one
        SearchingForTarget,

        //2 = Turn head to aim or Fire if within aim tolerance
        TurningTowardTarget,

        //3 = Fire 
        Firing,

    }

}
