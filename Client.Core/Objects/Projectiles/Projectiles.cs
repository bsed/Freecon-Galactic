using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Interfaces;
using Freecon.Client.Extensions;
using Freecon.Client.Managers;
using Freecon.Client.Objects.Structures;
using Freecon.Client.Objects.Pilots;
using Freecon.Models.TypeEnums;
using Core.Models.Enums;
using Freecon.Core.Utils;
using Server.Managers;

namespace Freecon.Client.Objects.Projectiles
{

    public abstract class Projectile<StatType> : IProjectile
        where StatType:ProjectileStats, new()
    {
        private float _angularVelocity;
        public float AngularVelocity
        {
            get { return 0; }//...wat?
            set { _angularVelocity = value; }
        }

        protected CollisionManager _collisionManager;
        
        public virtual Vector2 LinearVelocity { get; set; }
        
        protected virtual float _rotation { get; set; }
        public virtual float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public byte FiringWeaponSlot { get; set; }

        protected float _lastTimeStamp;
        
        protected float _creationTime;

        public Color DrawColor { get; set; }
        

        public ParticleEffectType TerminationEffect { get; set; }

        public float TerminationEffectSize { get; set; }

        virtual public Vector2 DrawPos { get { return ConvertUnits.ToDisplayUnits(Position); } }

        virtual public Vector2 Position { get; set; }

        public int Id { get; set; }

        protected virtual ProjectileBodyDataObject _bodyData{get;set;}
        public virtual CollisionDataObject BodyData { get { return _bodyData; } set { _bodyData = (ProjectileBodyDataObject)value; } }

        public bool IsBodyValid { get; set; }

        public StatType Stats { get; protected set; }//Flyweight stats
        protected double _lifetime { get { return Stats.Lifetime; } }
        public Texture2D Texture { get { return Stats.Texture; } }

        public virtual bool Enabled { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Projectile"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="stats">The stats.</param>
        public Projectile(CollisionManager collisionManager, int id, byte firingWeaponSlot)
        {
            _creationTime = TimeKeeper.MsSinceInitialization;
            _lastTimeStamp = TimeKeeper.MsSinceInitialization;
            Id = id;
            Stats = new StatType();
            FiringWeaponSlot = firingWeaponSlot;
            IsBodyValid = true;
            DrawColor = Color.White;
            Enabled = true;
            _collisionManager = collisionManager;
        }

        public virtual void Update(IGameTimeService gameTime)
        {
            if (TimeKeeper.MsSinceInitialization - _creationTime > _lifetime)
            {
                Terminate();
            }
           
        }
        
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);

            spriteBatch.Draw(Texture, DrawPos, null, DrawColor, Rotation, origin, 1, SpriteEffects.None, (int)DrawDepths.Projectile/1000f);
        }

        /// <summary>
        /// Terminates this instance.
        /// </summary>
        public virtual void Terminate()
        {
            // Tells projectile manager that we want to remove this laser
            IsBodyValid = false;
        }
        
    }
    
    /// <summary>
    /// Projectiles which do not have bodies and are manually updated with ray casts
    /// </summary>
    public class RayCastProjectile<StatType> : Projectile<StatType>
        where StatType:ProjectileStats, new()
    {        
        public Vector2 futurePos;

        protected World _world;
        /// <summary>
        /// The number recursive calls.
        /// Used to keep track of number of recursive tryUpdatePos calls, to prevent stack overflow
        /// </summary>
        protected uint numRecursiveCalls = 0;

        public RayCastProjectile(CollisionManager collisionManager,
            SpriteBatch spriteBatch, int ID, byte firingWeaponSlot, World world)
            : base(collisionManager, ID, firingWeaponSlot)
        {
            _world = world;

        }



        public override void Update(IGameTimeService gameTime)
        {
            // Kill projectile if it has been on the screen for its lifetime
            base.Update(gameTime);

            numRecursiveCalls = 0;

            double diff = TimeKeeper.MsSinceInitialization - _lastTimeStamp;

            // If the projectile is still around, update it
            futurePos = Position + (float)(TimeKeeper.MsSinceInitialization - _lastTimeStamp) * LinearVelocity;

            CheckCollisions();

            Position = futurePos;


            _lastTimeStamp = (float)TimeKeeper.MsSinceInitialization;
        }

        /// <summary>
        /// Checks for collisions. Handles collisions and calls terminate() as necessary
        /// </summary>
        public virtual void CheckCollisions()
        {
            if (numRecursiveCalls == 5)
            {
                Terminate();
                return;
            }

            numRecursiveCalls++;//Prevents stack overflow that can result when projectiles get stuck between very close reflectors


            if(LinearVelocity.Length() < 1e-3)//Might need to tweak this tolerance
            {
                //When the projectile velocity becomes very small, 
                //the raycast future position becomes tiny and it 
                //becomes almost impossible to collide. For now,
                //switch to a point test. TODO: Convert to multi-point test, testing bounds of projectile
                var hitFixtures = _world.TestPointAll(Position);
                foreach(var f in hitFixtures)
                {
                    if (f.Body.UserData != null)
                        HandleCollision((CollisionDataObject)f.Body.UserData, Vector2.Zero);
                }

            }
            
            if (futurePos != Position)
            {
                _world.RayCast(RayCastCallback, Position, futurePos);
            }
        }

        bool _AABBCallback(Fixture colidee)
        {
            if (colidee.Body.UserData != null)
            {
                HandleCollision((CollisionDataObject)colidee.Body.UserData, Vector2.Zero);
                return true;
            }
            else
            {
                Console.WriteLine("Warning: userdata is null for this object");
                return false;
            }
        }

        /// <summary>
        /// Handles the collision appropriately.
        /// Returns 0 if the raycast should be terminated
        /// Returns 1 if the raycast should not be terminated
        /// </summary>
        /// <returns></returns>
        protected virtual int HandleCollision(CollisionDataObject colideeObjectData, Vector2 normal)
        {
            if (colideeObjectData.Object.Enabled == false)//Ignore the collision if the body is disabled
                return 1;


            switch (((CollisionDataObject)colideeObjectData).BodyType)
            {
                case BodyTypes.PlayerShip:
                    return HandlePlayerCollision(colideeObjectData, normal);

                case BodyTypes.Sun:
                    return HandleSunCollision();

                case BodyTypes.NetworkShip:
                    return HandleNetworkShipCollision(colideeObjectData, normal);

                case BodyTypes.Turret:
                    return HandleTurretCollision(colideeObjectData, normal);

                case BodyTypes.WarpHole:
                    return HandleWarpholeCollision(colideeObjectData, normal);

                case BodyTypes.Missile:
                    return HandleMissileCollision(colideeObjectData, normal);

                default:
                    return HandleDefaultCollision(colideeObjectData, normal);


            }
        }

        private void ReflectAcrossNormal(Vector2 normalVector)
        {
            if (normalVector == Vector2.Zero)
                return;

            // Assuming n is the normal of the fixture at the point of the raycast contact
            // We just reflect the previous velocity accross the normal
            // Use angle - pi/2 for current angle

            float normalAngle = ((float)Math.Atan(normalVector.Y / normalVector.X) - (float)Math.PI / 2);

            Rotation += (2 * (normalAngle - (Rotation)));
            Rotation += (float)Math.PI;
            Vector2 oldvel = LinearVelocity;
            Vector2 tempVel;
            tempVel.X = (float)(LinearVelocity.Length() * Math.Sin(Rotation));
            tempVel.Y = -(float)(LinearVelocity.Length() * Math.Cos(Rotation));

            LinearVelocity = tempVel;
            // Manipulate velocity so that collisions reflect at 100% speed. Some weapons might not have this.
            LinearVelocity *= Math.Max(oldvel.Length() / LinearVelocity.Length(), 1f);

            futurePos = Position + (TimeKeeper.MsSinceInitialization - _lastTimeStamp) * LinearVelocity;

            CheckCollisions();
        }

        protected virtual int HandleSunCollision()
        {
            TerminationEffect = ParticleEffectType.ExplosionEffect;

            Terminate();
            return 0;
        }

        protected virtual int HandleNetworkShipCollision(CollisionDataObject other, Vector2 normal)
        {
            var shipData = (ShipBodyDataObject)other;

            if (_bodyData.FiringObj.IsBodyValid && shipData.Ship.IsBodyValid)
            {
                Ship firingShip = (Ship)_bodyData.FiringObj;
                Ship otherShip = shipData.Ship;


               
                if (firingShip != otherShip
                    && !firingShip.OnSameTeam(otherShip) && otherShip.CurrentHealth > 0)
                {
                    _collisionManager.ReportCollision(otherShip.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);
                    otherShip.Pilot.HitByTargetable(firingShip);

                    TerminationEffect = ParticleEffectType.LaserWaveEffect;


                    Terminate();
                    return 0;
                }

            }

            return 0;
        }

        /// <summary>
        /// Handles the player collision.
        /// Returns false if the projectile is terminated, true otherwise
        /// </summary>
        /// <param name="colidee">The colidee.</param>
        /// <returns></returns>
        protected virtual int HandlePlayerCollision(CollisionDataObject colidee, Vector2 normal)
        {
            var shipData = (ShipBodyDataObject)colidee;



            if (_bodyData.FiringObj is Ship)
            {
                if (_bodyData.FiringObj.IsBodyValid && shipData.Ship.IsBodyValid)
                {
                    Ship playerShip = shipData.Ship;
                    Ship firingShip = (Ship)_bodyData.FiringObj;

                    // If the laser is not from the playerShip, and the player is not currently dead
                    if (firingShip != playerShip
                        && ((PlayerPilot)playerShip.Pilot).IsAlive && !firingShip.OnSameTeam(playerShip))
                    {
                        _collisionManager.ReportCollision(playerShip.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);

                        playerShip.Pilot.CurrentTarget = firingShip;

                        TerminationEffect = ParticleEffectType.LaserWaveEffect;

                        Terminate();
                        return 0;
                    }
                }
            }
            else if (_bodyData.FiringObj is Turret)
            {
                //This should never happen, since we have a separate LaserTurret
                Turret t = (Turret)_bodyData.FiringObj;
                //t.ReportCollision() 
                Terminate();
                return 0;
            }


            return 1;
        }

        protected virtual int HandleTurretCollision(CollisionDataObject colidee, Vector2 normal)
        {

            Ship firingShip = (Ship)_bodyData.FiringObj;
            Turret turret = (Turret)((StructureBodyDataObject)colidee).Structure;


            if (turret.TurretType == TurretTypes.Planet)
            {
                if (!firingShip.OnSameTeam(turret))
                {
                    _collisionManager.ReportCollision(turret.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);


                    TerminationEffect = ParticleEffectType.LaserWaveEffect;

                    Terminate();
                    return 0;
                }
                else
                    return 1;


            }

            // If the firing turret and the player are on different teams and the player is not currently dead
            if (!turret.OnSameTeam(firingShip))
            {
                _collisionManager.ReportCollision(turret.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);


                TerminationEffect = ParticleEffectType.LaserWaveEffect;

                Terminate();
                return 0;
            }
            else
                return 1;


        }

        protected virtual int HandleWarpholeCollision(CollisionDataObject colidee, Vector2 normal)
        {
            //Do nothing
            return 0;
        }

        protected virtual int HandleMissileCollision(CollisionDataObject colidee, Vector2 normal)
        {
            //Do nothing
            return 0;
        }

        protected virtual int HandleDefaultCollision(CollisionDataObject colidee, Vector2 normal)
        {
            ReflectAcrossNormal(normal);
            return 0;
        }

        /// <summary>
        /// Ray cast callback.
        /// </summary>
        /// <param name="fixture">Fixture with which a collision has been detected</param>
        /// <param name="point">Point of collision</param>
        /// <param name="normal">Normal at point of collision on fixture</param>
        /// <param name="fraction">Fraction of ray at which collision occured</param>
        /// <returns></returns>
        private float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            //this callback function is called when a collision is detected
            if (fixture.Body.UserData != null)
            {
                return HandleCollision((CollisionDataObject)fixture.Body.UserData, normal);
            }
            else
            {
                Console.WriteLine("Warning: userdata is null for this object");
                return 0;
            }
        }
    
    }

    public class BodyProjectile<StatType> : Projectile<StatType>, IBodyProjectile
        where StatType : ProjectileStats, new()
    {

        protected override float _rotation { get { return Body.Rotation; } set { Body.Rotation = value; } }
        protected override ProjectileBodyDataObject _bodyData { get { return (ProjectileBodyDataObject)Body.UserData; } set { Body.UserData = value; } }
        public override Vector2 Position { get { return Body.Position; } set { Body.Position = value; } }

        public override Vector2 LinearVelocity { get { return Body.LinearVelocity; } set { Body.LinearVelocity = value; } }

        public Body Body { get; set; }

        public override bool Enabled { get { return Body.Enabled; } set { if(Body != null)Body.Enabled = value; } }

        public BodyProjectile(CollisionManager collisionManager, int id, byte firingWeaponSlot):base(collisionManager, id, firingWeaponSlot)
        {
           
        }

        public bool Body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if(fixtureB.Body.UserData != null)
            {
                return HandleCollision((CollisionDataObject)fixtureB.Body.UserData);
            }
            else
            {
                ConsoleManager.WriteLine("Warning: userdata is null for this object.", ConsoleMessageType.Warning);
                return false;
            }
        }

        protected virtual bool HandleCollision(CollisionDataObject colideeObjectData)
        {

            switch (((CollisionDataObject)colideeObjectData).BodyType)
            {
                case BodyTypes.PlayerShip:
                    return HandlePlayerCollision(colideeObjectData);

                case BodyTypes.Sun:
                    return HandleSunCollision();

                case BodyTypes.NetworkShip:
                    return HandleNetworkShipCollision(colideeObjectData);

                case BodyTypes.Turret:
                    return HandleTurretCollision(colideeObjectData);

                case BodyTypes.WarpHole:
                    return HandleWarpholeCollision(colideeObjectData);

                case BodyTypes.Missile:
                    return HandleMissileCollision(colideeObjectData);

                default:
                    return HandleDefaultCollision(colideeObjectData);


            }
        }


        protected virtual bool HandleSunCollision()
        {
            Terminate();
            return false;
        }

        protected virtual bool HandleNetworkShipCollision(CollisionDataObject other)
        {
            return false;
        }

        /// <summary>
        /// Handles the player collision.
        /// Returns false if the projectile is terminated, true otherwise
        /// </summary>
        /// <param name="colidee">The colidee.</param>
        /// <returns></returns>
        protected virtual bool HandlePlayerCollision(CollisionDataObject colidee)
        {           
            return false;
        }

        protected virtual bool HandleTurretCollision(CollisionDataObject colidee)
        {            
            return false;
        }

        protected virtual bool HandleWarpholeCollision(CollisionDataObject colidee)
        {
            //Do nothing
            return false;
        }

        protected virtual bool HandleMissileCollision(CollisionDataObject colidee)
        {
            //Do nothing
            return false;
        }

        protected virtual bool HandleDefaultCollision(CollisionDataObject colidee)
        {
            return true;
        }

    }
    


}