using Freecon.Client.Objects.Projectiles;
using Freecon.Client.Managers;
using Freecon.Client.Objects;
using Core.Models.Enums;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;

namespace Freecon.Client.Core.Objects.Projectiles
{
    /// <summary>
    /// Designed to be a unique projectile with 0ms lifetime. Creates an instantaneous AABB query, sends collision messages, and is removed.
    /// </summary>
    public class MineSplash : Projectile<MineSplashProjectileStats>
    {
        public MineSplash(Vector2 position, World w, CollisionManager collisionManager, int id, byte firingWeaponSlot
            ) : base(collisionManager, id, firingWeaponSlot)
        {
            
            //Detect splash damage to nearby ships
            AABB splashObj = new AABB(position, Stats.SplashRadius * 2f, Stats.SplashRadius * 2f);
            w.QueryAABB(MineSplash_OnCollision, ref splashObj);
            TerminationEffect = ParticleEffectType.ExplosionEffect;
            TerminationEffectSize = 4;
            Console.WriteLine("MineSplash Constructed");
        }

        bool MineSplash_OnCollision(Fixture f)
        {
            if (f.Body.UserData == null)
            {
                return true;
            }

            Console.WriteLine("Hit: " + f.Body.UserData.ToString());

            if (f.Body.UserData is ShipBodyDataObject)
            {
                //Note: Splash damages firing ship
                Ship hitShip = ((ShipBodyDataObject) f.Body.UserData).Ship;
                float splashStrength = (GetSplash(hitShip.Position));
                _collisionManager.ReportCollision(hitShip.Id, Id, Stats.ProjectileType, (byte) (splashStrength),
                    FiringWeaponSlot);

                //Knock away ship
                hitShip.Body.ApplyLinearImpulse(30*_getKnockawayVector(hitShip.Position, splashStrength));


            }
           
            


            //Always return true, make sure we get all overlapping objects
            return true;
        }


        protected virtual float GetSplash(Vector2 hitObjPos)
        {
            float dist = (Position - hitObjPos).Length();

            if (dist <= 1f)
                return 1;
            else if (dist < Stats.SplashRadius) //linear taper to 0
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
            return 3*pctStrenght * (hitObjectPos - Position);
        }
    }
    
}