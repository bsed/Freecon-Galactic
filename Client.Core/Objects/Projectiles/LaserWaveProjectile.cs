using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using System;
using Freecon.Client.Extensions;
using Freecon.Core.Utils;
using Server.Managers;

namespace Freecon.Client.Objects.Projectiles
{
    public class LaserWaveProjectile : LaserProjectile
    {

        float lastEffectDrawTime = 0;
        ParticleManager _particleManager;
        Vector2 _initialVelocity;

        float _zeroFraction;//Avoids a cast


        /// <summary>
        /// Initializes a new instance of the <see cref="LaserWaveProjectile"/> class.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="creationTime">The creation time.</param>
        /// <param name="type">The type.</param>
        /// <param name="ID">The identifier.</param>
        /// <param name="stats">The stats.</param>
        /// <param name="world">The world.</param>
        public LaserWaveProjectile(CollisionManager collisionManager, ParticleManager particleManager, Vector2 initialVelocity,
            SpriteBatch spriteBatch, int ID, byte firingWeaponSlot, World world)
            : base(collisionManager, spriteBatch, ID, firingWeaponSlot, world)
        {
            Stats = new LaserWaveProjectileStats();
            DrawColor = Color.Fuchsia;
            _particleManager = particleManager;
            _initialVelocity = initialVelocity;
            _zeroFraction = ((LaserWaveProjectileStats)Stats).ZeroFraction;
        }


        public override void Update(IGameTimeService gameTime)
        {
            base.Update(gameTime);
            

            float liveTime = (float)TimeKeeper.MsSinceInitialization - _creationTime;

            //Keeping it simple, linear decrease in velocity to 0
            if (liveTime / _lifetime < _zeroFraction)
            {
                float newX = _initialVelocity.X + ((-(1 / _zeroFraction) * _initialVelocity.X / (float)_lifetime) * liveTime); ;
                float newY = _initialVelocity.Y + ((-(1 / _zeroFraction) * _initialVelocity.Y / (float)_lifetime) * liveTime); ;
                LinearVelocity = new Vector2(newX, newY);
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (TimeKeeper.MsSinceInitialization - lastEffectDrawTime > ((LaserWaveProjectileStats)Stats).DrawPeriod)
            {
                _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(Position), ((LaserWaveProjectileStats)Stats).DrawEffectType, 2);
                lastEffectDrawTime = (float)TimeKeeper.MsSinceInitialization;
            }
        }

        protected override int HandleCollision(CollisionDataObject colideeObjectData, Vector2 normal)
        {
            base.HandleCollision(colideeObjectData, normal);

            if (colideeObjectData.BodyType == BodyTypes.PlayerShip)
            {
                //Pass through player ship
                return 1;
            }
            else
            {
                Terminate();
                return 0;//Always terminate
            }
        }

        protected override int HandleMissileCollision(CollisionDataObject colidee, Vector2 normal)
        {
            IMissile m = (IMissile)(((ProjectileBodyDataObject)(colidee)).Bullet);

            try
            {
                if (((ProjectileBodyDataObject)BodyData).FiringObj is ITeamable && !m.OnSameTeam((ITeamable)((ProjectileBodyDataObject)BodyData).FiringObj))
                {
                    m.Health -= 25;

                    if (m.Health <= 0)
                        m.Terminate();

                    Terminate();
                    return 0;
                }
                return 1;


            }
            catch (Exception e)
            {
                ConsoleManager.WriteLine("Error during collision between LaserWave and Missile: ", ConsoleMessageType.Error);
                ConsoleManager.WriteLine(e.ToString(), ConsoleMessageType.Error);
                return 1;
            }
        }

    }
}
