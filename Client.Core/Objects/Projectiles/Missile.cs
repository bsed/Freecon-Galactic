using System;
using System.Collections.Generic;
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
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using Freecon.Models.TypeEnums;
using Core.Models.Enums;
using Freecon.Core.Utils;


namespace Freecon.Client.Objects.Projectiles
{
    public class Missile<StatType> : BodyProjectile<StatType>, ITargeter, ISimulatable, IMissile
        where StatType:MissileProjectileStats, new()
    {
        public override Vector2 LinearVelocity { get { return Body.LinearVelocity; } set { Body.LinearVelocity = value; } }
        ParticleManager _particleManager;

        float _particlePeriod = 1;//ms, period between drawing of thrust particle effect
        float _lastParticleDrawTime = 0;//ms

        public ITargetable CurrentTarget { get { return _pilot.CurrentTarget; } set { _pilot.CurrentTarget = value; } }

        public bool IsLocalSim { get; set; }

        public Dictionary<int, ITargetable> PotentialTargets { get; set; }

        World _world;

        MissilePilotLv1 _pilot;

        public float CurrentTurnRate { get { return _currentTurnRate; } }
        float _currentTurnRate;

        float _currentThrust;

        public string name;

        //Just for fun right now, but good proof of concept.
        public float Health { get; set; }

        public StatType Stats;

        public ICanFire FiringObj { get; set; }

        override public Vector2 Position { get { return Body.Position; } set { Body.Position = value; } }

        override public Vector2 DrawPos { get { return ConvertUnits.ToDisplayUnits(Body.Position); } }

        public HashSet<int> Teams { get; set; }

        public bool IsAlliedWithPlanetOwner { get; set; }

        //protected override float _rotation { get { return _body.Rotated; } set { _body.Rotated = _rotation; } }
        //public override float Rotated { get { return _body.Rotated; } set { _body.Rotated = value; } }


        public Missile(CollisionManager collisionManager, World w, int ID, ICanFire firingObj, bool isLocalSim, ParticleManager pm, byte firingWeaponSlot)
            : base(collisionManager, ID, firingWeaponSlot)
        {
            Stats = new StatType();

            Debugging.AddStack.Push(this.ToString());
            Body = BodyFactory.CreateCircle(w, .1f, 10);
            Body.IsKinematic = true;
            Body.IsBullet = false;
            Body.IsStatic = false;
            Body.UserData = new ProjectileBodyDataObject(BodyTypes.Missile, ID, firingObj, this);
            Body.OnCollision += _body_OnCollision;
            _world = w;

            FiringObj = firingObj;

            _particleManager = pm;

            Health = 100;
          

            TerminationEffect = ParticleEffectType.ExplosionEffect;
            IsLocalSim = isLocalSim;
            //_pilot = new MissilePilot(this, 5);
            _pilot = new MissilePilotLv2(this, 5);

            PotentialTargets = new Dictionary<int, ITargetable>();
            Teams = firingObj.Teams;
            _currentTurnRate = Stats.BaseTurnRate;
            _currentThrust = Stats.BaseThrust;
            _creationTime = TimeKeeper.MsSinceInitialization;
        }

        bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            
            if (fixtureB.Body.UserData is ShipBodyDataObject)
            {
                var s = fixtureB.Body.UserData as ShipBodyDataObject;

                if (s.ID != FiringObj.Id)
                {
                    if(FiringObj is ITargetable)
                        s.Ship.Pilot.HitByTargetable((ITargetable)FiringObj);
                    Terminate();
                    return true;
                }
                else
                    return false;//Don't collide with firing ship
            }
            else if (fixtureB.Body.UserData is ProjectileBodyDataObject)
            {
                var s = fixtureB.Body.UserData as ProjectileBodyDataObject;
                if (s.FiringObj.Id != FiringObj.Id && s.BodyType == BodyTypes.Missile)//Destroyed by other missiles
                {
                    Terminate();
                    return false;

                }
                else
                {
                    return false;//Don't collide
                }
            }
            else if(fixtureB.Body.UserData is StructureBodyDataObject)
            {
                var s = fixtureB.Body.UserData as StructureBodyDataObject;
                if(s.Structure.OnSameTeam(this))
                {
                    return false;
                }
                else
                {
                    Terminate();
                    return false;
                }

            }
            
            
            Terminate();//Kill projectile on collision with anything else
            return false;
        }
        
        public override void Update(IGameTimeService gameTime)
        {
            base.Update(gameTime);
            //_pilot.Update(gameTime);    

           
            _rotation = (-(float)Math.Atan2(Body.LinearVelocity.X, Body.LinearVelocity.Y) + (float)Math.PI);
           
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (TimeKeeper.MsSinceInitialization - _lastParticleDrawTime > _particlePeriod)
            {
                _particleManager.TriggerEffect(DrawPos, ParticleEffectType.MissileEffect, 1);
                _particleManager.TriggerEffect(DrawPos, ParticleEffectType.MissileEffect, 1);//WARNING: Drawing twice to enhance effect, should probably modify actual effect
                _lastParticleDrawTime = TimeKeeper.MsSinceInitialization;
            }
        
        }

        public void Simulate(IGameTimeService gameTime)
        {
            _currentTurnRate = Stats.BaseTurnRate + (float)Math.Pow(Math.E, (gameTime.TotalMilliseconds - _creationTime) / 1000f - 1f);//e^(x-2)/2, function I pulled out of my ass, missile turns faster and faster as it ages
            

            if (Body.LinearVelocity.Length() >= Stats.BaseSpeed)
                Body.LinearDamping = Stats.SpeedDampValue;
            else
                Body.LinearDamping = .0001f;

            _pilot.Simulate(gameTime);

            if (_pilot.CurrentTarget != null && (Body.Position - _pilot.CurrentTarget.Position).Length() < .1f)
                Terminate();
        }
        
        public void ThrustForward()
        {

            //float forceAngle = (-(float)Math.Atan2(-forceY, forceX) + (float)Math.PI / 2);
  
            float forceX = (float)((_currentThrust) * Math.Sin(Rotation)) * 2;
            float forceY = (float)((_currentThrust) * Math.Cos(Rotation)) * 2;


            Body.ApplyForce(new Vector2((forceX), (-forceY)));
        }

        /// <summary>
        /// Applies either acceleration, or maximum allowed according to thrust and weight (F=ma)
        /// </summary>
        /// <param name="acceleration"></param>
        public void CorrectVelocity(Vector2 acceleration)
        {
            Vector2 forceReq = acceleration * Body.Mass;
            
            
            if(forceReq.Length() > 3f * _currentThrust)
            {
                forceReq.Normalize();
                forceReq *= _currentThrust;
            }

            Body.ApplyForce(forceReq);

        }

        public override void Terminate()
        {
            if (!IsBodyValid)
                return;//Important, prevents stack overflow if this missile kills another missile

            base.Terminate();

            //Detect splash damage to nearby ships
            AABB splashObj = new AABB(Position, Stats.SplashRadius * 2f, Stats.SplashRadius * 2f);        
            _world.QueryAABB(MissileSplash_OnCollision, ref splashObj);
            

 

        }
        bool MissileSplash_OnCollision(Fixture f)
        {
            if(f.Body.UserData is ShipBodyDataObject)
            {
                //Note: Splash damages firing ship
                Ship hitShip = ((ShipBodyDataObject)f.Body.UserData).Ship;
                float splashStrength = (GetSplash(hitShip.Position));
                _collisionManager.ReportCollision(hitShip.Id, Id, Stats.ProjectileType, (byte)(splashStrength), FiringWeaponSlot);

                //Knock away ship
                hitShip.Body.ApplyLinearImpulse(30* _getKnockawayVector(hitShip.Position, splashStrength));


            }
            else if(f.Body.UserData is ProjectileBodyDataObject)
            {
                ProjectileBodyDataObject hitProj = (ProjectileBodyDataObject)f.Body.UserData;
                float splashStrength = GetSplash(hitProj.Bullet.Position);
                //For now, let's just blow up other missiles, or knock them away, we can add health later
                if (hitProj.Bullet is IMissile)
                {
                    IMissile m = (IMissile)hitProj.Bullet;

                    m.Body.ApplyLinearImpulse(_getKnockawayVector(hitProj.Bullet.Position, splashStrength/10f));
                }
            }
            else if (f.Body.UserData is StructureBodyDataObject)
            {
                var s = f.Body.UserData as StructureBodyDataObject;
                if (!(s.Structure is CommandCenter))
                {
                    float splashStrength = (GetSplash(s.Structure.Position));
                    _collisionManager.ReportCollision(s.Structure.Id, Id, Stats.ProjectileType, (byte)(splashStrength), FiringWeaponSlot);
                }
            }


            //Always return true, make sure we get all overlapping objects
            return true;
        }             

        
        private float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed)
        {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // Get the arch tangent: y/x
            var desiredAngle = (float)Math.Atan2(y, x);

            float difference = WrapAngle(desiredAngle - currentAngle);

            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            return WrapAngle(currentAngle + difference);
        }

        private float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        //Returns a value between 1 and 0 representing splash damage amount.
        protected virtual float GetSplash(Vector2 hitObjPos)
        {
            float dist = (Position - hitObjPos).Length();

            if (dist <= .5f)
                return 1;
            else if (dist < Stats.SplashRadius)//linear taper to 0
                return -dist + Stats.SplashRadius;
            else
                return 0;

        }

        /// <summary>
        /// Returns the vector pointing in the direction of force to apply to knock away the body
        /// </summary>
        /// <returns></returns>
        protected virtual Vector2 _getKnockawayVector(Vector2 hitObjectPos, float pctStrenght)
        {
            return pctStrenght * (hitObjectPos - Position) * Stats.KnockForce;
        }
    }

    //public class AmbassadorMissile : Missile<AmbassadorProjectileStats>
    //{
    //    public AmbassadorMissile(CollisionManager collisionManager, World w, int ID, ICanFire firingObj, bool isLocalSim, ParticleManager pm, byte firingWeaponSlot)
    //        : base(collisionManager, w, ID, firingObj, isLocalSim, pm, firingWeaponSlot)
    //    {
            
    //    }

    //}

    //public class HellhoundMissile : Missile<HellhoundProjectileStats>
    //{
    //    public HellhoundMissile(CollisionManager collisionManager, World w, int ID, ICanFire firingObj, bool isLocalSim, ParticleManager pm, byte firingWeaponSlot)
    //        : base(collisionManager, w, ID, firingObj, isLocalSim, pm, firingWeaponSlot)
    //    {
           
    //    }

    //}

    //public class MissileType1 : Missile<MissileType1ProjectileStats>
    //{
    //    public MissileType1(CollisionManager collisionManager, World w, int ID, ICanFire firingObj, bool isLocalSim, ParticleManager pm, byte firingWeaponSlot)
    //        : base(collisionManager, w, ID, firingObj, isLocalSim, pm, firingWeaponSlot)
    //    {
       
    //    }

    //}

    //public class MissileType2 : Missile<MissileType2ProjectileStats>
    //{
    //    public MissileType2(CollisionManager collisionManager, World w, int ID, ICanFire firingObj, bool isLocalSim, ParticleManager pm, byte firingWeaponSlot)
    //        : base(collisionManager, w, ID, firingObj, isLocalSim, pm, firingWeaponSlot)
    //    {
           
    //    }

    //}

    //public class MissileType3 : Missile<MissileType3ProjectileStats>
    //{
    //    public MissileType3(CollisionManager collisionManager, World w, int ID, ICanFire firingObj, bool isLocalSim, ParticleManager pm, byte firingWeaponSlot)
    //        : base(collisionManager, w, ID, firingObj, isLocalSim, pm, firingWeaponSlot)
    //    {
     
    //    }

    //}

    //public class MissileType4 : Missile<MissileType4ProjectileStats>
    //{
    //    public MissileType4(CollisionManager collisionManager, World w, int ID, ICanFire firingObj, bool isLocalSim, ParticleManager pm, byte firingWeaponSlot)
    //        : base(collisionManager, w, ID, firingObj, isLocalSim, pm, firingWeaponSlot)
    //    {
           
    //    }

    //}



}
