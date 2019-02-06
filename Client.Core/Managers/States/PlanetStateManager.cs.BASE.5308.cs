using System;
using System.Diagnostics;
using System.Collections.Generic;

using FarseerPhysics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using SRClient.GUI;
using SRClient.Mathematics;
using SRClient.Objects;
using SRClient.Managers.Networking;
using SRClient.Managers.GUI;
using Lidgren.Network;
using SRClient.Interfaces;
using SRClient.BusMessages;
using MemBus;
using SRClient.Extensions;
using FarseerPhysics.Factories;
using SRClient.Objects.Ships;
using SRClient.Objects.Structures;
using System.IO;
using AwesomiumUiLib;
using System.Reflection;
using SRClient.Managers.GUI.States;
using FreeconClient.Core;
using Freecon.Models.TypeEnums;
using SRClient.Base;


namespace SRClient.Managers.States
{
    public class PlanetStateManager : GameState, IGameState, IHandle<MChangeStateMessage>
    {
        private PlanetLevel level; // The layout containing walls, turrets, buildings, etc
        public HashSet<int> ColonyTeamIDs = new HashSet<int>();
        private Stopwatch updateWatch, drawWatch;
        private bool ColonizeMode = false; // This is temporary
        private bool StructurePlacementMode = false; // Maybe not temporary after the new IoC and bus framework

        protected ClientManager _clientManager;
        protected HudElementManager _hudElementManager;
        protected ProjectileManager _projectileManager;
        protected ShipManager _shipManager;
        protected SpriteBatch _spriteBatch;
        protected TextureManager _textureManager;
        protected WarpHoleManager _warpHoleManager;
        protected SpaceStateManager _spaceStateManager;
        protected MessageManager _messageManager;
        protected IBus _bus;
        protected DebugTextManager _debugTextManager;

        public List<BaseUI> Windows { get; set; }

        public Dictionary<int, ITargetable> PotentialTurretTargets { get; set; } // Any ships that are not allied with the planet owner, used by turrets to find targets

        //Debugging, to give ships motion, although these could be used in the game in interesting ways
        List<GravityObject> _gravityObjects = new List<GravityObject>();

        private IList<Turret> _turrets;
        public IList<Turret> Turrets
        {
            get { return _turrets; }
            private set { _turrets = value; }
        }

        public override float Zoom
        {
            get { return Camera.Zoom; }
            set { Camera.Zoom = value; }
        }

        /// <summary>
        /// Manages all aspects of Planetary Invasion.
        /// </summary>
        /// <param name="Content">Content Manager</param>
        /// <param name="planetLevel">Level to play on.</param>
        public PlanetStateManager(SpriteBatch spriteBatch,
                                  ClientManager clientManager,
                                  HudElementManager hudElementManager,
                                  ParticleManager particleManager,
                                  PhysicsManager physicsManager,
                                  ProjectileManager projectileManager,
                                  ShipManager shipManager,
                                  TextureManager textureManager,
                                  WarpHoleManager warpHoleManager,
                                  SpaceStateManager spaceStateManager,
                                  MessageManager messageManager,
                                  IBus bus,
                                  DebugTextManager debugTextManager,
                                  TargetingManager targetManager,
                                  TeamManager teamManager,
                                  ConfigPlanetStateUI uiConfig,
                                  GameWindow window)

            : base(particleManager, physicsManager, targetManager, teamManager)
        {
            _clientManager = clientManager;
            _spriteBatch = spriteBatch;
            _hudElementManager = hudElementManager;
            _shipManager = shipManager;
            _textureManager = textureManager;
            _projectileManager = projectileManager;
            _warpHoleManager = warpHoleManager;
            _spaceStateManager = spaceStateManager;
            _messageManager = messageManager;
            _bus = bus;
            _debugTextManager = debugTextManager;
            _UI = new PlanetStateUI(uiConfig, shipManager.GetPlayerEnergy);

            _UI.RegisterInstructionCallback("ToggleColonizeMode", ToggleColonizeMode);
            _UI.RegisterInstructionCallback("TurretClicked", ToggleStructurePlacementMode);
            _UI.RegisterInstructionCallback("MissileClicked", _UIFireAmbassador);

            Turrets = new List<Turret>();

            Camera = new Camera2D();
            updateWatch = new Stopwatch();
            drawWatch = new Stopwatch();
            Windows = new List<BaseUI>();
            Camera.Zoom = 1f;

            PotentialTurretTargets = new Dictionary<int, ITargetable>();

            _bus.Subscribe(this);

            this.OnStructureRemoved += PlanetStateManager_OnStructureRemoved;
        }





        void _UIFireAmbassador()
        {
            Utilities.ColoredConsoleWriteLine("UI: Attempting to fire missile...", ConsoleMessageType.Notification);
            _shipManager.PlayerShip.TryFireMissile(_lastTimeStamp, ProjectileTypes.AmbassadorMissile);
        }


        void PlanetStateManager_OnStructureRemoved(object sender, StructureRemovedEventArgs e)
        {
            if (e.RemovedStructure != null && e.RemovedStructure is Turret)
            {
                RemoveTurret(e.RemovedStructure as Turret);
            }
        }

        private void RemoveTurret(Turret t)
        {
            Turrets.Remove(t);
        }

        public void SetState()
        {
            _hudElementManager.SetWindowList(Windows);
        }

        public void LoadPlanetLevel(
            PlanetTypes planetType,
            IEnumerable<IEnumerable<Vector2>> islands, 
            int height, 
            int width,
            bool[] layoutArray)
        {
            level = new PlanetLevel(_spriteBatch, _textureManager, _physicsManager, planetType, islands, height, width, layoutArray);
        }

        public void Update(GameTime gameTime)
        {
#if ADMIN
            foreach (GravityObject g in _gravityObjects) // Testing stuff
            {
                g.Gravitate(_shipManager.ShipList.Values, _shipManager.PlayerShip);
            }
#endif
            if (KeyboardManager.ColonizeMode.IsBindTapped() && !ColonizeMode)
            {
                ColonizeMode = true;
            }
            else if (KeyboardManager.ColonizeMode.IsBindTapped() && ColonizeMode)
            {
                ColonizeMode = false;
            }

            level.Update(gameTime, _shipManager.PlayerShip.Position, _shipManager.GetPlayerShipDifference());
            
            Camera.Pos = ConvertUnits.ToDisplayUnits(_shipManager.PlayerShip.Position);

            if (MouseManager.ScrolledUp)
            {
                Camera.Zoom += .1f;
            }

            if (MouseManager.ScrolledDown)
            {
                Camera.Zoom -= .1f;
            }

            MathHelper.Clamp(Camera.Zoom, 0.1f, 1f); 

            if (ColonizeMode)
            {
                if (MouseManager.RightMouseClicked)
                {
                    ColonizeMode = false;
                }

                // Temporary, send colonize request
                if (MouseManager.LeftMouseClicked)
                {
                    Vector2 buildingPos = MousePosToSimUnits(_shipManager.PlayerShip, _spriteBatch);
                    _bus.Publish(new MSendStructurePlacementRequest(buildingPos, StructureTypes.CommandCenter));
                }
            }

            if (StructurePlacementMode)
            {
                // Cancel with right click
                if (MouseManager.RightMouseClicked)
                {
                    StructurePlacementMode = false;
                }

                // Temporary, send turret placement request
                if (MouseManager.LeftMouseClicked)
                {
                    var buildingPos = MousePosToSimUnits(_shipManager.PlayerShip, _spriteBatch);

                    _bus.Publish(new MSendStructurePlacementRequest(buildingPos, StructureTypes.LaserTurret));
                }
            }

            foreach (var s in _structures)
            {
                switch (s.buildingType)
                {
                    case StructureTypes.LaserTurret:
                        ((Turret)s).Update(gameTime, _bus);
                        break;

                    default:
                        s.Update();
                        break;
                }
            }

            _debugTextManager.Update(gameTime);

            _UI.Update(gameTime);
            _lastTimeStamp = gameTime.TotalGameTime.TotalMilliseconds;
        }

        public void Draw(GameTime gameTime)
        {
            // Draw the tiles efficiently, with the camera viewport.
            _spriteBatch.Begin(
                SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                null,
                null,
                Camera.get_transformation(_spriteBatch.GraphicsDevice));

            level.DrawTiles(Camera);
            

            _spriteBatch.End();

            // Draw everything else, with the camera viewport.
            _spriteBatch.Begin(
                SpriteSortMode.BackToFront, BlendState.Additive,
                null,
                null,
                null,
                null,
                Camera.get_transformation(_spriteBatch.GraphicsDevice));

            _particleManager.Draw(_spriteBatch);

            _spriteBatch.End();

            _spriteBatch.Begin(
                SpriteSortMode.FrontToBack, BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp,
                null,
                null,
                null,
                Camera.get_transformation(_spriteBatch.GraphicsDevice));
            
            foreach (var s in _structures)
            {
                switch (s.buildingType)
                {
                    case StructureTypes.LaserTurret:
                        ((Turret)s).Draw(_spriteBatch);
                        break;

                    default:
                        s.Update();
                        break;
                }
            }

            foreach (GravityObject g in _gravityObjects)
            {
                g.Draw(_spriteBatch);
                g.TriggerParticleEffect(_particleManager, gameTime);
            }


            _projectileManager.Draw(Camera.Pos);

            _shipManager.Draw();

            // Structures
            foreach (var s in _structures)
            {
                s.Draw(_spriteBatch);
            }

            // Temporary
            if (ColonizeMode)
            {
                _spriteBatch.Draw(_textureManager.Penguin, MousePosToDisplayUnits(Camera._zoom, _shipManager.PlayerShip, _spriteBatch), Color.White);
            }
            
            _spriteBatch.End();

            // Draw some debug text, relative to the screen's pixels.
            _spriteBatch.Begin();
            
#if ADMIN
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 8, "Bullets: " + GetProjectileCount());
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 9, "FPS: " + Debugging.DebugTextManager.getFPS());
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 25, "Bullets: " + _projectileManager.GetProjectileCount());
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 26, "Structures: " + _structures.Count);
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 27, "Latency: " + (MainNetworkingManager.LastLatency * 1000) + "ms");
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 28, "NPCs: " + Debugging.SimulationManager.GetSimulatedShips().Count);
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 29, "Player Speed: " + LegacyStatics.playerShipManager.PlayerShip.LinearVelocity.Length());
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 30, "Player Location: " + LegacyStatics.playerShipManager.PlayerShip.Position.ToString());
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 31, "Mouse Location: " + MousePosToSimUnits(_shipManager.PlayerShip, _spriteBatch));
            Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 32, "Bodies: " + _physicsManager.World.BodyList.Count);
#endif

           


            _spriteBatch.End();

            _UI.Draw(_spriteBatch);

            




        }

        /// <summary>
        /// Returns the farseer display coordinates of the current mouse position
        /// </summary>
        /// <returns></returns>
        public Vector2 MousePosToDisplayUnits(float zoom, Ship playerShip, SpriteBatch spriteBatch)
        {
            return MousePosToSimUnits(playerShip, spriteBatch) * 100f;
        }

        public void DebugDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            _shipManager.Debug(spriteBatch);


            level.Debug(spriteBatch);

            spriteBatch.End();
        }

        private void Clear()
        {

            ColonizeMode = false;
            StructurePlacementMode = false;
            _structures.Clear();
            targetsAvailable = false;
            PotentialTurretTargets.Clear();
            _targetManager.Clear(_shipManager.PlayerShip);
            ColonyTeamIDs = new HashSet<int>();
        }

        public void CreateWarphole(float xPos, float yPos, byte warpIndex)
        {
            _warpHoleManager.CreateWarphole(xPos, yPos, warpIndex);
        }

        public override void CreateStructure(
            float xPos,
            float yPos,
            StructureTypes structureType,
            float health,
            float constructionPoints,
            int ID,
            HashSet<int> teams)
        {
            Structure structure;

            switch (structureType)
            {
                case (StructureTypes.LaserTurret):
                    /*Turret t = new PlanetTurret(_projectileManager, _textureManager, _shipManager, new Vector2(xPos, yPos), buildingType, _bus, _physicsManager, health, ID, 666);
                    structures.Add(ID, t);
                    t.potentialTargets = _potentialTargets;
                    t.isLocalSim = localSim;                    */
                    

                    Turret t = new Turret(_projectileManager, _textureManager, _shipManager, _bus, _physicsManager.World, new Vector2(xPos, yPos), structureType, health, ID, 666, TurretTypes.Planet, teams);
                    _teamManager.RegisterObject(t);
                    _structures.Add(t);
                    Turrets.Add(t);
                    t.IsAlliedWithPlanetOwner = true;//True by default, may change later

                    structure = t;
                    break;
                    
                case(StructureTypes.CommandCenter):
                {
                    structure = new CommandCenter(_bus, _messageManager.SendEnterColonyRequest, _shipManager.IsEnterModeOn, _physicsManager.World, _textureManager, new Vector2(xPos, yPos), structureType, health, ID, teams);
                    _teamManager.RegisterObject(structure);
                    _structures.Add(structure);

                    break;
                }
                default:
                    structure = new Structure(_bus, _textureManager, xPos, yPos, structureType, health, ID, teams);

                    // Need to move this
                    _structures.Add(structure);

                    structure.Body = BodyFactory.CreateCircle(_physicsManager.World, 1, 1, new StructureBodyDataObject(BodyTypes.Structure, ID, structure));

                    break;
            }

            _targetManager.RegisterObject(structure);
            
        }
        /// <summary>
        /// Toggles colonize mode, ensures that no other modes are active
        /// This will need to be changed to allow for placement of other structures
        /// </summary>
        public void ToggleColonizeMode()
        {
            ColonizeMode = !ColonizeMode;

            if (ColonizeMode)
                StructurePlacementMode = false;
        }

        /// <summary>
        /// Toggles colonize mode, ensures that no other modes are active
        /// This will need to be changed to allow for placement of other structures
        /// </summary>
        public void ToggleStructurePlacementMode()
        {
            StructurePlacementMode = !StructurePlacementMode;

            if (StructurePlacementMode)
                ColonizeMode = false;
        }

        public int GetProjectileCount()
        {
            return _projectileManager.GetProjectileCount();
        }

        public void Handle(MChangeStateMessage msg)
        {
            if (msg.State == GameStates.Planet)
                Clear();
        }

        public override void KillStructure(int ID)
        {
            var structure = GetStructureByID(ID);

            if (structure != null)
            {

                Vector2 effectPos = new Vector2(structure.xPos, structure.yPos);

                _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(effectPos), EffectType.ExplosionEffect, 5);

                //Just some proof of concept fun
                List<Vector2> rp = Utilities.GetRandomPointsInRadius(effectPos, 5, 0f, 1.5f);
                foreach (var v in rp)
                {
                    _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(v), EffectType.SmokeTrailEffect, 5);                    
                }
                rp = Utilities.GetRandomPointsInRadius(effectPos, 5, 0f, 1.5f);
                foreach (var v in rp)
                {
                    _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(v), EffectType.SmallFlameExplosionEffect, 5);
                }


                
                if (structure.Body != null)
                    structure.Body.Dispose();

                _targetManager.DeRegisterObject(structure);

                if (structure is ITeamable)
                    _teamManager.DeRegisterObject((ITeamable)structure);

                _structures.Remove(structure);
            }
        }

        public void LoadTestPlanet(PlanetTypes planetType)
        {
            bool[] layoutArray = new bool[] { false, false, false, false, false, false,
                                       false, true , true , true , true , false,
                                       false, true , false, false, true , false,
                                       false, true , false, false, true , false,
                                       false, true , true , true , true , false,
                                       false, false, false, false, false, false };



            // No walls
            //bool[] layoutArray = new bool[] {
            //                           false, false, false, false, false, false,
            //                           false, false , false, false, false, false,
            //                           false, false , false, false, false, false,
            //                           false, false , false, false, false, false,
            //                           false, false , false , false,false, false,
            //                           false, false, false, false, false, false };
            
            // Random islands, not currently implemented anyway
            List<List<Vector2>> islands = new List<List<Vector2>>();
            
            int height = 6;
            int width = 6;

            LoadPlanetLevel(planetType, islands, height, width, layoutArray);

            //_gravityObjects.Add(new GravityObject(new Vector2(-5, -5), 1));
            //_gravityObjects.Add(new GravityObject(new Vector2(-5, 5), 1));
            //_gravityObjects.Add(new GravityObject(new Vector2(5, 5), 1));
            //_gravityObjects.Add(new GravityObject(new Vector2(5, -5), 1));
            _gravityObjects.Add(new GravityObject(new Vector2(0, 0), 1));

            CreateStructure(3, 3, StructureTypes.LaserTurret, 10000, 100, 546345, new HashSet<int>());
            _turrets[0].IsLocalSim = true;
            Debugging.SimulationManager.StartSimulating(_turrets[0]);
            //Ship npc = _shipManager.CreateShip(new Vector2(-1, -1), 7, 0, Vector2.Zero, "NPC", ShieldTypes.halo, ShipTypes.NPC_Penguin, WeaponTypes.Laser, WeaponTypes.Laser, _shipManager.PlayerShip.Teams, false);
            
            //npc.IsLocalSim = true;
            //Debugging.SimulationManager.StartSimulating(npc);
            
            _shipManager.PlayerShip.AddStaticCargo(CargoTypes.AmbassadorMissile, 999);
           
        }

        
    }

    class GravityObject
    {
        Vector2 _position;
        float _gravVal;

        float lastTriggerTime = 0;
        float drawPeriod = 10;//ms

        public GravityObject(Vector2 position, float gravVal)
        {
            _position = position;
            _gravVal = gravVal;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.greenPoint, ConvertUnits.ToDisplayUnits(_position), Color.White);
        }

        public void Gravitate(IEnumerable<Ship> ships, Ship ignoreShip)
        {
            foreach(Ship s in ships)
            {
                if (s == ignoreShip)
                    continue;

                Vector2 forceDir = (s.Position - _position);
                float dist = forceDir.Length();
                forceDir.Normalize();

                if (dist < .5f)
                    dist = .5f;

                forceDir = -forceDir;
                
                s.Body.ApplyLinearImpulse(forceDir * _gravVal / (dist * dist));//Inverse square gravity
                
            }

        }

        public void TriggerParticleEffect(ParticleManager pm, GameTime gameTime)
        {
            if(gameTime.TotalGameTime.TotalMilliseconds - lastTriggerTime > drawPeriod)
            {
                lastTriggerTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
                pm.TriggerEffect(ConvertUnits.ToDisplayUnits(_position), EffectType.WarpHoleEffect, .2f);
            }
        }
    }
}