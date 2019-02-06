using System;
using System.Collections.Generic;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Interfaces;
using Freecon.Client.Objects.Projectiles;
using Freecon.Client.Objects.Structures;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.Objects.Projectiles;
using Freecon.Client.Objects;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Core.Objects.Weapons;
using Freecon.Client.Mathematics;
using Freecon.Core;
using Server.Managers;

namespace Freecon.Client.Managers
{
    /// <summary>
    /// Main class that manages bullets, missiles, lasers, and various other projectiles.
    /// </summary>
    public partial class ProjectileManager
        : ISynchronousUpdate, IDraw
    {

        private ParticleManager _particleManager;
        private World _world;
        private SpriteBatch _spriteBatch;
        private TargetingService _targetingManager;
        private SimulationManager _simulationManager;
        MessageService_ToServer _messageService;
        CollisionManager _collisionManager;


        // Projectile Object Pooling Variables
        public List<IProjectile> _projectileList = new List<IProjectile>();


        //Projectile Flyweights
        private Dictionary<ProjectileTypes, ProjectileStats> _flyweights;
      
       
        public ProjectileManager(ParticleManager particleManager,
                                 World world,
                                 SpriteBatch spriteBatch,
                                 TargetingService targetingManager,
                                 SimulationManager simulationManager,
                                 MessageService_ToServer messageService,
                                 CollisionManager collisionManager
                                 )
        {

            _particleManager = particleManager;
            _world = world;
            _spriteBatch = spriteBatch;
            _targetingManager = targetingManager;
            _simulationManager = simulationManager;
            _messageService = messageService;
            _collisionManager = collisionManager;

            _projectileList = new List<IProjectile>(1000);

            //TODO: Get rid of this nonsense
            _flyweights = new Dictionary<ProjectileTypes, ProjectileStats>();
            _flyweights.Add(ProjectileTypes.Laser, new LaserProjectileStats());
            _flyweights.Add(ProjectileTypes.LaserWave, new LaserWaveProjectileStats());
            _flyweights.Add(ProjectileTypes.PlasmaCannon, new PlasmaCannonProjectileStats());
            _flyweights.Add(ProjectileTypes.NaniteLauncher, new NaniteLauncherProjectileStats());
            _flyweights.Add(ProjectileTypes.AmbassadorMissile, new AmbassadorProjectileStats());
            _flyweights.Add(ProjectileTypes.HellHoundMissile, new HellhoundProjectileStats());
            _flyweights.Add(ProjectileTypes.MissileType1, new MissileType1ProjectileStats());
            _flyweights.Add(ProjectileTypes.MissileType2, new MissileType2ProjectileStats());
            _flyweights.Add(ProjectileTypes.MissileType3, new MissileType3ProjectileStats());
            _flyweights.Add(ProjectileTypes.MissileType4, new MissileType4ProjectileStats());

            _flyweights.Add(ProjectileTypes.BC_Laser, new BC_LaserProjectileStats());
            _flyweights.Add(ProjectileTypes.GravityBomb, new GravityBombProjectileStats());
            _flyweights.Add(ProjectileTypes.MineSplash, new MineSplashProjectileStats());


            
            
        }

        public void Update(IGameTimeService gameTime)
        {

            for (int i = 0; i < _projectileList.Count; i++)
            {
                if (!_projectileList[i].IsBodyValid)
                {
                    if (_particleManager != null)//Gross but necessary for simulator, TODO: find workaround to avoid null check when terminating projectiles
                    {
                        _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(_projectileList[i].Position),
                            _projectileList[i].TerminationEffect, _projectileList[i].TerminationEffectSize);
                    }
                    if (_projectileList[i] is IBodyProjectile)
                    {
                        Debugging.DisposeStack.Push(this.ToString());
                        ((IBodyProjectile)_projectileList[i]).Body.Dispose();
                    }

                    _projectileList.RemoveAt(i);

                    i--;

                    continue;
                }

                _projectileList[i].Update(gameTime);
            }
        }

        public void Draw(Camera2D camera)
        {
            for (int i = 0; i < _projectileList.Count; i++ )
            {
                //if (IsOnScreen(_spriteBatch, camPosition, ConvertUnits.ToDisplayUnits(_projectileList[i].Position)))
                //{
                //}

                // Todo: Render only onscreen
                _projectileList[i].Draw(_spriteBatch);
            }
        }

        /// <summary>
        /// Checks if a given projectile is currently on the scrren
        /// BROKEN
        /// </summary>
        /// <param name="camPosition"></param>
        /// <param name="projectile"></param>
        /// <returns></returns>
        private bool IsOnScreen(SpriteBatch sb, Vector2 camPosition, Vector2 p)
        {            
                        
            return p.X > (camPosition.X - sb.GraphicsDevice.Viewport.Width / 2)
                && p.Y > (camPosition.Y - sb.GraphicsDevice.Viewport.Height / 2)
                && p.X < (camPosition.X + sb.GraphicsDevice.Viewport.Width / 2)
                && p.Y < (camPosition.Y + sb.GraphicsDevice.Viewport.Height / 2);
        }

        /// <summary>
        /// Gets the angle between 2 points.
        /// </summary>
        /// <returns>Returns the angle.</returns>
        private float GetAngle(Vector2 oldPosition, Vector2 Position)
        {
            float xDiff = MathHelper.Distance(oldPosition.X, Position.X);
            float yDiff = MathHelper.Distance(oldPosition.Y, Position.Y);
            float dist = (int)Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
            float angle = 0;

            // Exceptions
            if (xDiff == 0)
            {
                if (yDiff > 0) return 90;
                return 270;
            }
            else if (yDiff == 0)
            {
                if (xDiff > 0) return 0;
                return 180;
            }

            // Quadrant detection
            int quad = GetQuadrant(xDiff, yDiff);

            //Calculate angle//
            angle = (int)Math.Round(MathHelper.ToDegrees((float)Math.Asin(xDiff / dist)));

            //Quadrant compensation//
            if (quad < 3)
                angle = 90 - angle;
            else
                angle = 270 + angle;

            return angle;
        }

        private int GetQuadrant(float xDiff, float yDiff)
        {
            int quad = 1;
            if (xDiff < 0 && yDiff > 0)
                quad = 2;
            else if (xDiff < 0 && yDiff < 0)
                quad = 3;
            else if (xDiff > 0 && yDiff < 0)
                quad = 4;
            return quad;
        }

        /// <summary>
        /// Clears all projectiles.
        /// </summary>
        public void Clear()
        {
            //_bodyList.Clear();
            _projectileList.Clear();

        }

        #region Creating Projectiles

        public void CreateMissile(ProjectileTypes projectileType , 
                                Vector2 position, Vector2 velocityOffset, float rotation,
                                int projectileID, ICanFire firingObj, bool isLocalSim, byte firingWeaponSlot)
        {
            IMissile proj = null;
            
            
            if(firingObj is Turret)
            {
                ConsoleManager.WriteLine("ERROR: Missiles not implemented for turrets in ProjectileManager.CreateMissile().", ConsoleMessageType.Error);
            }
            else 
            {
                switch (projectileType)
                {                  

                    case ProjectileTypes.AmbassadorMissile:
                        {
                            proj = new Missile<AmbassadorProjectileStats>(_collisionManager, _world, projectileID, firingObj, isLocalSim, _particleManager, firingWeaponSlot);
                            _simulationManager.StartSimulating(proj);
                            _targetingManager.RegisterObject(proj);
                            break;
                        }
                

                    case ProjectileTypes.HellHoundMissile:
                        {
                            proj = new Missile<HellhoundProjectileStats>(_collisionManager, _world, projectileID, firingObj, isLocalSim, _particleManager, firingWeaponSlot);
                            _simulationManager.StartSimulating(proj);
                            _targetingManager.RegisterObject(proj);
                            break;
                        }

                    case ProjectileTypes.MissileType1:
                        {
                            proj = new Missile<MissileType1ProjectileStats>(_collisionManager, _world, projectileID, firingObj, isLocalSim, _particleManager, firingWeaponSlot);
                            _simulationManager.StartSimulating(proj);
                            _targetingManager.RegisterObject(proj);
                            break;
                        }
                    case ProjectileTypes.MissileType2:
                        {
                            proj = new Missile<MissileType2ProjectileStats>(_collisionManager, _world, projectileID, firingObj, isLocalSim, _particleManager, firingWeaponSlot);
                            _simulationManager.StartSimulating(proj);
                            _targetingManager.RegisterObject(proj);
                            break;
                        }
                    case ProjectileTypes.MissileType3:
                        {
                            proj = new Missile<MissileType3ProjectileStats>(_collisionManager, _world, projectileID, firingObj, isLocalSim, _particleManager, firingWeaponSlot);
                            _simulationManager.StartSimulating(proj);
                            _targetingManager.RegisterObject(proj);
                            break;
                        }
                    case ProjectileTypes.MissileType4:
                        {
                            proj = new Missile<MissileType4ProjectileStats>(_collisionManager, _world, projectileID, firingObj, isLocalSim, _particleManager, firingWeaponSlot);
                            _simulationManager.StartSimulating(proj);
                            _targetingManager.RegisterObject(proj);
                            break;
                        }

                    default:
                        ConsoleManager.WriteLine("ERROR: ProjectileType " + projectileType + " not implemented in ProjectileManager.CreateProjectile().", ConsoleMessageType.Error);
                        break;
                }
                velocityOffset = new Vector2(velocityOffset.X / 1000f + (float)(Math.Sin(rotation)) * _flyweights[projectileType].BaseSpeed / 1000f,
                                         velocityOffset.Y / 1000f - (float)(Math.Cos(rotation)) * _flyweights[projectileType].BaseSpeed / 1000f);

                //Proj could be null here, but it should be an easy catch if we forget to add a switch case and it breaks
                proj.BodyData = new ProjectileBodyDataObject(BodyTypes.Projectile, projectileID, firingObj, proj);
            }




            proj.LinearVelocity = velocityOffset;
            proj.Rotation = rotation;
            proj.Position = position;
            ((IProjectile)proj).Id = projectileID;
            _projectileList.Add(proj);
        }
          

        public void CreateProjectile(ProjectileTypes type, 
                            Vector2 position, Vector2 velocityOffset, float rotation,
                            int projectileID, ICanFire firingObj, float charge, byte firingWeaponSlot)
        {
            IProjectile proj = null;

            Vector2 projectileVelocity = new Vector2(velocityOffset.X/1000f + (float)(Math.Sin(rotation)) * _flyweights[type].BaseSpeed / 1000f,
                                        velocityOffset.Y/1000f  - (float)(Math.Cos(rotation)) * _flyweights[type].BaseSpeed / 1000f);
                                    

            if (firingObj is Turret)
            {
                proj = new TurretLaser(_collisionManager, _spriteBatch, projectileID, firingWeaponSlot, ((Turret)firingObj).TurretType, _world);

                proj.BodyData = new ProjectileBodyDataObject(BodyTypes.Projectile, projectileID, firingObj, proj);

            }
            else 
            {
                switch (type)
                {
                    case ProjectileTypes.Laser:
                        proj = new LaserProjectile(_collisionManager, _spriteBatch, projectileID, firingWeaponSlot, _world);
                        
                        break;

                    case ProjectileTypes.LaserWave:
                        proj = new LaserWaveProjectile(_collisionManager, _particleManager, projectileVelocity, _spriteBatch, projectileID, firingWeaponSlot, _world);
                        
                        break;

                    case ProjectileTypes.PlasmaCannon:
                        proj = new PlasmaCannonProjectile(_collisionManager, _spriteBatch, projectileID, firingWeaponSlot, _world, charge);
                        
                        break;
                    case ProjectileTypes.BC_Laser:
                        proj = new BC_LaserProjectile(_collisionManager, _spriteBatch, projectileID, firingWeaponSlot, _world);
                        break;
                    case ProjectileTypes.GravityBomb:
                        proj = new GravityBombProjectile(_collisionManager, _particleManager, projectileVelocity, firingObj, _world, projectileID, firingWeaponSlot, position);
                        break;
                    case ProjectileTypes.MineSplash:
                        proj = new MineSplash(position, _world, _collisionManager, projectileID, firingWeaponSlot);
                        break;
                    default:
                        ConsoleManager.WriteLine("ERROR: ProjectileType " + type + " not implemented in ProjectileManager.CreateProjectile().", ConsoleMessageType.Error);
                        break;
                }

                // Proj could be null here, but it should be an easy catch if we forget to add a switch case and it breaks
                proj.BodyData = new ProjectileBodyDataObject(BodyTypes.Projectile, projectileID, firingObj, proj);
    
            }

            


            proj.LinearVelocity = projectileVelocity;
            proj.Rotation = rotation;
            proj.Position = position;
            proj.Id = projectileID;

            _projectileList.Add(proj);
        }

        #endregion

        #region Collision Handling

        private bool bulletBody_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureA.Body.UserData != null && fixtureB.Body.UserData != null)
            {
                CollisionDataObject userData = (CollisionDataObject)fixtureA.Body.UserData;
                if (userData.BodyType == BodyTypes.VerticalBorder || userData.BodyType == BodyTypes.HorizontalBorder)
                {
                    return true;
                }


                //BROKEN AND PROBABLY OUTDATED ANYWAY

                #region Missiles

                //                else if (userData.type == BodyTypes.PlayerShip)
                //                {
                //                    ID = userdata.Remove(0, 7);
                //                    int index = int.Parse(ID);
                //#if DEBUG
                //                    Console.WriteLine(ID);
                //#endif
                //                    projectileList[index].setLife(0);//Set life to 0 to destroy the missile

                //                    return false;
                //                }
                //                else if (userdata.StartsWith("Planet"))
                //                {
                //                    ID = userdata.Remove(0, 7);
                //                    int index = int.Parse(ID);
                //#if DEBUG
                //                    Console.WriteLine(ID);
                //#endif
                //                    projectileList[index].setLife(0);//Set life to 0 to destroy the missile
                //                    return false;
                //                }
                //                else if (userdata.StartsWith("NetworkShip"))
                //                {
                //                    ID = userdata.Remove(0, 7);
                //                    int index = int.Parse(ID);
                //#if DEBUG
                //                    Console.WriteLine(ID);
                //#endif
                //                    projectileList[index].setLife(0);//Set life to 0 to destroy the missile
                //                    return false;
                //                }

                #endregion
            }

            return true;
        }

        #endregion

        public void CreateProjectiles(ProjectileRequest request)
        {
            if (request.SendToServer)
            {
                if (request.FiringObj is Structure)
                {
                    _messageService.SendStructureFireRequest(request.FiringObj.Id, request.FiringRotation, request.GetProjectileIDs(), request.FiringWeaponSlot, request.PctCharge);
                }
                else if (request.FiringObj is Ship)
                {
                    _messageService.SendShipFireRequest(request.FiringObj.Id, request.FiringWeaponSlot, request.GetProjectileIDs(), request.FiringRotation, request.PctCharge, request.ProjectileType);
                }
            }

            if (!Validation.IsMissileType(request.ProjectileType))
            {
                foreach (var pd in request.ProjectileData)
                {
                    CreateProjectile(request.ProjectileType, pd.ProjectilePosition, pd.VelocityOffset, pd.ProjectileRotation, pd.ProjectileID, request.FiringObj, pd.PctCharge, request.FiringWeaponSlot);
                }

            }
            else
            {
                foreach (var pd in request.ProjectileData)
                {
                    CreateMissile(request.ProjectileType, pd.ProjectilePosition, pd.VelocityOffset, pd.ProjectileRotation, pd.ProjectileID, request.FiringObj, true, request.FiringWeaponSlot);
                }

            }

        }




        public int GetProjectileCount()
        {
            return _projectileList.Count;
        }        
        
    }
}