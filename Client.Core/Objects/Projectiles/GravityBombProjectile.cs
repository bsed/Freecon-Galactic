using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Freecon.Client.Core.Objects;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Freecon.Core.Utils;

namespace Freecon.Client.Objects.Projectiles
{
    public class GravityBombProjectile:BodyProjectile<GravityBombProjectileStats>
    {
        ParticleManager _particleManager;
        GravityObject _gravityObject;
        List<Body> bodiesInArea;

        List<float> flipPoints;
        int _currentFlipPoint = 0;

        public GravityBombProjectile(CollisionManager collisionManager, ParticleManager particleManager, Vector2 velocity, ICanFire firingObj, World world, int id, byte firingWeaponSlot, Vector2 position)
            : base(collisionManager, id, firingWeaponSlot)
        {
            _particleManager = particleManager;
            Debugging.AddStack.Push(this.ToString());
            Body = BodyFactory.CreateCircle(world, 1, 1);
            Body.UserData = new ProjectileBodyDataObject(BodyTypes.Projectile, id, firingObj, this);
            Body.OnCollision += Body_OnCollision;
            Body.LinearVelocity = velocity;       
            


            _gravityObject = new GravityObject(position, Stats.InitialGravityVal);
            bodiesInArea = new List<Body>();

            //This is kind of cheating, but it should be OK for now. Just make sure to check if the body is valid (check body.IsDisposed) before doing anything with it to avoid farseer exceptions
            foreach(Body b in world.BodyList)
            {
                CollisionDataObject bdo = b.UserData as CollisionDataObject;

                if(bdo!= null)
                {
                    if(bdo.BodyType == BodyTypes.Ship || bdo.BodyType == BodyTypes.NetworkShip || bdo.BodyType == BodyTypes.PlayerShip || bdo.BodyType == BodyTypes.Missile)
                    {                        
                        bodiesInArea.Add(b);
                    }

                }
            }


            flipPoints = new List<float>(Stats.NumFlips);
            for(int i = 0; i < Stats.NumFlips; i++)
            {
                flipPoints.Add(Stats.NonGravityFraction + ((1 - Stats.NonGravityFraction) / Stats.NumFlips * (i+1)));
            }

            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gravityObject.TriggerParticleEffect(_particleManager);
        }

        public override void Update(IGameTimeService gameTime)
        {
            base.Update(gameTime);

            float lifeFraction = (TimeKeeper.MsSinceInitialization - _creationTime)/Stats.Lifetime;

            float gravFraction = 1 - Stats.NonGravityFraction;

            _gravityObject.Position = Position;
            if(lifeFraction > Stats.NonGravityFraction)
                _gravityObject.Gravitate(bodiesInArea);

            //Check if its time to flip gravity
            if (_currentFlipPoint < flipPoints.Count)
            {
                if (lifeFraction > flipPoints[_currentFlipPoint])
                {
                    float gravVal = (Stats.FinalGravityVal - Stats.InitialGravityVal) / Stats.NumFlips * (_currentFlipPoint + 1);
                    _gravityObject.GravVal *= -1 * Math.Sign(_gravityObject.GravVal) * gravVal;
                    _currentFlipPoint++;
                }
            }


        }

        
        



    }
}
