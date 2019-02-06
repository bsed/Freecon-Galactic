using Core.Models.Enums;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Objects.Pilots;
using Freecon.Client.Objects.Structures;
using Freecon.Client.Extensions;
using FarseerPhysics.Dynamics;

namespace Freecon.Client.Objects.Projectiles
{
    public class TurretLaser : LaserProjectile
    {

        TurretTypes _turretType;

        public TurretLaser(CollisionManager collisionManager,
            SpriteBatch spriteBatch,
            int ID,
            byte firingWeaponSlot,
            TurretTypes turretType,
            World world)
            : base(collisionManager, spriteBatch, ID, firingWeaponSlot, world)
        {
            DrawColor = Color.Red;
            _turretType = turretType;

        }

        protected override int HandleNetworkShipCollision(CollisionDataObject other, Vector2 normal)
        {
            var shipData = (ShipBodyDataObject)other;

            Turret firingTurret = (Turret)_bodyData.FiringObj;
            Ship hitShip = shipData.Ship;

            if (_turretType == TurretTypes.Planet)
            {
                if (!hitShip.OnSameTeam(firingTurret))
                {
                    _collisionManager.ReportCollision(hitShip.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);

                    hitShip.Pilot.HitByTargetable(firingTurret);

                    TerminationEffect = ParticleEffectType.LaserWaveEffect;

                    Terminate();
                    return 0;

                }

                return 1;
            }
            else if (!firingTurret.OnSameTeam(hitShip) && hitShip.CurrentHealth > 0)
            {
                _collisionManager.ReportCollision(hitShip.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);

                hitShip.CurrentTarget = firingTurret;

                hitShip.Pilot.HitByTargetable(firingTurret);

                TerminationEffect = ParticleEffectType.LaserWaveEffect;

                Terminate();

                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Handles the player collision.
        /// Returns false if the projectile is terminated, true otherwise
        /// </summary>
        /// <param name="colidee">The colidee.</param>
        /// <returns></returns>
        protected override int HandlePlayerCollision(CollisionDataObject colidee, Vector2 normal)
        {
            var shipData = (ShipBodyDataObject)colidee;

            Turret T = (Turret)_bodyData.FiringObj;


            Ship playerShip = shipData.Ship;

            if (_turretType == TurretTypes.Planet)
            {
                if (!playerShip.OnSameTeam(T))
                {
                    _collisionManager.ReportCollision(playerShip.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);


                    TerminationEffect = ParticleEffectType.LaserWaveEffect;

                    Terminate();
                    return 0;

                }
                else
                    return 1;


            }

            // If the firing turret and the player are on different teams and the player is not currently dead
            else if (((PlayerPilot)playerShip.Pilot).IsAlive && !T.OnSameTeam(playerShip))
            {
                _collisionManager.ReportCollision(playerShip.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);


                TerminationEffect = ParticleEffectType.LaserWaveEffect;

                Terminate();
                return 0;
            }
            else
                return 1;
        }

        protected override int HandleTurretCollision(CollisionDataObject colidee, Vector2 normal)
        {

            Turret firingTurret = (Turret)_bodyData.FiringObj;
            Turret hitTurret = (Turret)((StructureBodyDataObject)colidee).Structure;

            if (_turretType == TurretTypes.Planet)
            {
                return 1;//Do nothing, we currently only allow allied turrets on a planet


            }
            else if (!hitTurret.OnSameTeam(firingTurret))
            {
                _collisionManager.ReportCollision(hitTurret.Id, Id, Stats.ProjectileType, 0, FiringWeaponSlot);


                TerminationEffect = ParticleEffectType.LaserWaveEffect;
                Terminate();
                return 0;
            }
            else
                return 1;


        }
    }
}
