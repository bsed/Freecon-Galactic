using System;
using System.Collections.Generic;

using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SRClient.GUI;
using SRClient.Managers.GUI;
using SRClient.Mathematics;
using SRClient.Mathematics.Effects;
using SRClient.Mathematics.Space;
using SRClient.Objects;
using SRClient.Interfaces;
using SRClient.BusMessages;
using MemBus;
using Microsoft.Xna.Framework.Input;
using SRClient.Extensions;
using SRClient.Objects.Structures;
using Freecon.Models.TypeEnums;
using SRClient.Managers.GUI.States;
using SRClient.Base;



namespace SRClient.Managers.States
{
    public class SpaceStateManager : GameState, IGameState, IHandle<MChangeStateMessage>
    {
        // ----------- //
        // Sub Classes //
        // ----------- //

        public List<string> ConnectionInfo;
        protected RotationalCamera rotationalCamera;
        protected HudElementManager _hudElementManager;


        protected SpriteBatch _spriteBatch;
        protected ContentManager _content;
        protected GraphicsDeviceManager _graphics;

        private bool StructurePlacementMode = false;


        #region Manager Dependencies
        protected IBus _bus;
        protected ChatManager _chatManager;
        protected ProjectileManager _projectileManager;
        protected RadarSpaceManager _radarSpaceManager;
        protected ShipManager _shipManager;
        protected SpaceManager _spaceManager;
        protected WarpHoleManager _warpholeManager;
        protected TextureManager _textureManager;
        #endregion

        // Loading //
        public GameStates State;
        public List<BaseUI> Windows;

        // Camera
        protected bool cameraFollowing;
        public Vector2 shipPos;
        public float shipRotation;
        protected float timeBetween;


       
        public SpaceStateManager(IBus bus,
                                 ContentManager Content, 
                                 GraphicsDeviceManager graphics, 
                                 SpriteBatch spriteBatch,
                                 ChatManager chatManager,
                                 HudElementManager hudElementManager,
                                 ParticleManager particleManager,
                                 PhysicsManager physicsManager,
                                 ProjectileManager projectileManager,
                                 ShipManager shipManager,
                                 SpaceManager spaceManager,
                                 WarpHoleManager warpholeManager,
                                 TextureManager textureManager,
                                 TargetingManager targetManager,
                                 TeamManager teamManager,
                                 ConfigSpaceStateUI uiConfig)
            : base(particleManager, physicsManager, targetManager, teamManager)
        {
            State = GameStates.loading;
            _bus = bus;
            _spaceManager = spaceManager;
            _chatManager = chatManager;
            _hudElementManager = hudElementManager;
            _physicsManager = physicsManager;
            _projectileManager = projectileManager;
            _shipManager = shipManager;
            _spriteBatch = spriteBatch;
            _warpholeManager = warpholeManager;
            _textureManager = textureManager;
            _UI = new SpaceStateUI(uiConfig, uiConfig.GameWindow, shipManager.GetPlayerEnergy);


            
            _UI.RegisterInstructionCallback("TurretClicked", ToggleStructurePlacementMode);
            _UI.RegisterInstructionCallback("MissileClicked", _UIFireAmbassador);



            rotationalCamera = new RotationalCamera(chatManager);
            Camera = new Camera2D();

            Windows = new List<BaseUI>();

            _bus.Subscribe(this);

            State = GameStates.updating;
        }

        public void SetState()
        {
            Reset();

            //if (GameStateManager.getState() == GameStates.motherShipBattle)
            //    _hudElementManager.SetWindowList(_MSBManager.Windows);
            //else
                _hudElementManager.SetWindowList(Windows);

            _chatManager.renderChat = true;
            //ProjectileManager.initialize(1000, PhysicsManager.world);
        }

        private void Reset()
        {

            _hudElementManager.SetWindowList(Windows);
            
            _spaceManager.Reset();
            _targetManager.Clear(_shipManager.PlayerShip);
            

            _structures.Clear();
        }

        public Vector2 getShipPos
        {
            get { return shipPos; }
        }

        public float getShipRotation
        {
            get { return _shipManager.PlayerShip.Rotation; }
        }

        public void Update(GameTime gameTime, bool isActive)
        {
            switch (State)
            {
                case GameStates.loading:
                    break;
                case GameStates.updating:
                    shipPos = _shipManager.PlayerShip.Position;
                    shipRotation = _shipManager.PlayerShip.Rotation;

                    Camera.Pos = ConvertUnits.ToDisplayUnits(_shipManager.PlayerShip.Position);
                    Camera.UpdateCameraShake();

                    rotationalCamera.UpdateCamera(Camera, null, ref cameraFollowing);

                    // Get Data for Minimap
                    //_spaceManager.convertPlanets(_radarSpaceManager.planetList);
                        // Should be the most optimized route. This should be checked later, previously it leaked. WARNING

                    State = _spaceManager.getState();
                    _spaceManager.Update(gameTime, _shipManager.EnterMode, _shipManager.PlayerShip.Position,
                                        _shipManager.GetPlayerShipDifference());

                    _shipManager.SetOldPosition(ConvertUnits.ToDisplayUnits(_shipManager.PlayerShip.Position));

                    

                    if (StructurePlacementMode)
                    {
                        if (MouseManager.RightButtonPressed)
                            StructurePlacementMode = false;//Cancel with right click

                        if (MouseManager.LeftButtonPressed)
                        {//Temporary, send turret placement request

                            Vector2 buildingPos = MousePosToSimUnits(_shipManager.PlayerShip, _spriteBatch);
                            _bus.Publish(new MSendStructurePlacementRequest(buildingPos, StructureTypes.LaserTurret));

                        }


                    }

                    foreach (var s in _structures)
                    {
                        switch (s.buildingType)
                        {
                            case StructureTypes.LaserTurret:
                                
                                var tur = (Turret)s;

                                tur.Update(gameTime, _bus);
                                break;

                            default:
                                s.Update();
                                break;

                        }
                    }

                    _UI.Update(gameTime);

                    break;
                case GameStates.transitional:
                    _bus.Publish(new MChangeStateMessage(GameStates.Port));
                    break;
            }
            _lastTimeStamp = gameTime.TotalGameTime.TotalMilliseconds;
        }

        public void Draw(BloomComponent bloom, bool isActive, bool isBloom)
        {
            switch (State)
            {
                case GameStates.loading:
                    _spriteBatch.Begin();
                    //DebugTextManager.DrawTextToScreenLeft(_spriteBatch, 3, "Loading Level");
                    _spriteBatch.End();
                    break;

                case GameStates.updating:

                    // Sometimes there is a discrepency due to methods calling world update
                    Camera.Pos = ConvertUnits.ToDisplayUnits(_shipManager.PlayerShip.Position);

                    if (isBloom)
                    {
                        _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                        _spriteBatch.Draw(bloom.screenTarget, Vector2.Zero, Color.White);

                        _spriteBatch.End();


                        _spriteBatch.Begin();
                        
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 25, "Bullets: " + _projectileManager.GetProjectileCount());
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 26, "Structures: " + _structures.Count);
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 27, "Latency: " + (MainNetworkingManager.LastLatency * 1000) + "ms");
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 28, "NPCs: " + Debugging.SimulationManager.GetSimulatedShips().Count);
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 29, "Player Speed: " + LegacyStatics.playerShipManager.PlayerShip.LinearVelocity.Length());
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 30, "Player Location: " + LegacyStatics.playerShipManager.PlayerShip.Position.ToString());



                        Vector2 mousePoss = MousePosToSimUnits(_shipManager.PlayerShip, _spriteBatch);

                        //float shipToMousAngle = AIHelper.GetRotationToPosition(_shipManager.PlayerShip.Position, mousePoss, _shipManager.PlayerShip.Rotation) * 180 / (float)Math.PI;
                        float shipToMousAngle = AIHelper.GetAngleToPosition(_shipManager.PlayerShip.Position, mousePoss) * 180 / (float)Math.PI;
                        float shipAngle = AIHelper.ClampRotation(_shipManager.PlayerShip.Rotation) * 180 / (float)Math.PI;
                        //AIHelper.TurnTowardPosition(ref mouseToShipAngle, .10f, mousePoss, _shipManager.PlayerShip.Position, .00001f, 10f);
                        float angleToRotate = AIHelper.GetRotationToPosition(_shipManager.PlayerShip.Position, mousePoss, _shipManager.PlayerShip.Rotation) * 180 / (float)Math.PI;

                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 31, "Angle, Ship To Mouse: " + shipToMousAngle);
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 32, "Ship Angle, clamped: " + shipAngle);
                        Debugging.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 33, "Angle to rotate: " + angleToRotate);
                        //Main.DebugTextManager.DrawTextToScreenRight(_spriteBatch, 26, "FPS: " + Main.DebugTextManager.getFPS());


                        _spriteBatch.End();

                    }

                    _UI.Draw(_spriteBatch);

                    break;
            }
        }

        public void DrawBloom(GameTime gameTime, BloomComponent bloom, bool isBloom)
        {
            if (isBloom)
                bloom.BeginDraw(_spriteBatch);
            _spaceManager.DrawBackGround(cameraFollowing, Camera.Pos);

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                              null,
                              null,
                              null,
                              null,
                              Camera.get_transformation(_spriteBatch.GraphicsDevice));

            _spaceManager.Draw(_spriteBatch, Camera.Pos, Zoom);

            _spriteBatch.End();

            // Draw particles in different spritebatch. Otherwise they look absolutely awful.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive,
                              null,
                              null,
                              null,
                              null,
                              Camera.get_transformation(_spriteBatch.GraphicsDevice));
            _spaceManager.DrawWarpZones();
            _particleManager.Draw(_spriteBatch);
            _shipManager.ParticleDraw(_spriteBatch);

            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                              null,
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

            _projectileManager.Draw(Camera._pos);
            _shipManager.Draw();
            //ParticleManager.DrawOnTop(spriteBatch); // Not needed atm. Needed when we use Charge effects, etc
            _spriteBatch.End();
            //normal.Draw(spriteBatch, gameTime, ConvertUnits.ToDisplayUnits(shipPos), spaceCam);
#if DEBUG
            //_spriteBatch.Begin();
            //string the = ""+ShipManager.getPlayerShipDifference();
            ////DebugTextManager.DrawTextToScreenLeft(_spriteBatch, 2, the);
            ////ProjectileManager.Debug(_spriteBatch);
            //_spriteBatch.End();
#endif
            if (isBloom)
                bloom.Draw(_spriteBatch);
        }

        /// <summary>
        /// Returns a bool that states if the cursor is within the window or not.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        private bool isInWindow(SpriteBatch spriteBatch)
        {
            if (MouseManager.CurrentPosition.X >= 0 && MouseManager.CurrentPosition.Y >= 0
                && MouseManager.CurrentPosition.X < spriteBatch.GraphicsDevice.Viewport.Width
                && MouseManager.CurrentPosition.Y < spriteBatch.GraphicsDevice.Viewport.Height)
            {
                return true;
            }
            return false;
        }

        public void CreatePlanet(int distance, int maxTrip, PlanetTypes type, float currentTrip, float scale, int ID,
                                 int parentID, bool isMoon)
        {
            _spaceManager.CreatePlanet(distance, maxTrip, type, currentTrip, scale, ID, parentID, isMoon);
        }

        public void CreateWarphole(float xpos, float ypos, byte warpIndex)
        {
            _spaceManager.CreateWarphole(xpos, ypos, warpIndex);
        }

        public override void CreateStructure(float xPos, float yPos, StructureTypes buildingType, float health,
                                  float constructionPoints, int ID, HashSet<int> teams)
        {
            switch (buildingType)
            {

                case (StructureTypes.LaserTurret):
                    Turret t = new Turret(_projectileManager, _textureManager, _shipManager, _bus, _physicsManager.World, new Vector2(xPos, yPos), buildingType, health, ID, 666, TurretTypes.Space, teams);
                    _teamManager.RegisterObject(t);
                    _structures.Add(t);
                    t.Teams = teams;
                    break;

                default:

                    _structures.Add(new Structure(_bus, _textureManager, xPos, yPos, buildingType, health, ID, teams));
                    break;
            }
        }

        public void CreateBorder(int SizeOfSystem)
        {
            _spaceManager.CreateBorderAndGravity(SizeOfSystem);
        }

        public void CreatePort(int distance, int maxTrip, PlanetTypes type, float currentTrip, float scale, int ID,
                               int parentID, bool isMoon)
        {
            _spaceManager.CreatePort(distance, maxTrip, type, currentTrip, scale, ID, parentID, isMoon);
        }

        public void ClearSystem()
        {
            _spaceManager.ClearSystem();
        }

        public void ToggleStructurePlacementMode()
        {
            StructurePlacementMode = !StructurePlacementMode;
        }

        void _UIFireAmbassador()
        {
            Utilities.ColoredConsoleWriteLine("UI: Attempting to fire missile...", ConsoleMessageType.Notification);
            _shipManager.PlayerShip.TryFireMissile(_lastTimeStamp, ProjectileTypes.AmbassadorMissile);
        }

        public Vector2 MousePosToDisplayUnits(float zoom)
        {
            Vector2 mousePos = new Vector2();
            mousePos.X = _shipManager.PlayerShip.Position.X + (MouseManager.CurrentPosition.X - _spriteBatch.GraphicsDevice.Viewport.Width / 2f) / (_textureManager.Ground.Width * zoom);
            mousePos.Y = _shipManager.PlayerShip.Position.Y + (MouseManager.CurrentPosition.Y - _spriteBatch.GraphicsDevice.Viewport.Height / 2f) / (_textureManager.Ground.Height * zoom);

            return ConvertUnits.ToDisplayUnits(mousePos);
        }

        /// <summary>
        /// Returns the farseer sim coordinates of the current mouse position
        /// </summary>
        /// <returns></returns>
        //public Vector2 MousePosToSimUnits(float zoom)
        //{
            //Vector2 mousePos = new Vector2();
            //mousePos.X = _shipManager.PlayerShip.Position.X + (MouseManager.CurrentPosition.Y - _spriteBatch.GraphicsDevice.Viewport.Width / 2f) / (_textureManager.Ground.Width * zoom);
            //mousePos.Y = _shipManager.PlayerShip.Position.Y + (MouseManager.CurrentPosition.Y - _spriteBatch.GraphicsDevice.Viewport.Height / 2f) / (_textureManager.Ground.Height * zoom);
            //return mousePos;

        //}

       
       
        public void Handle(MChangeStateMessage msg)
        {
            if(msg.State == GameStates.Space)
                Reset();


        }
              
        public override void KillStructure(int ID)
        {
            var structure = GetStructureByID(ID);

            if (structure != null)
            {

                Vector2 effectPos = new Vector2(structure.xPos, structure.yPos);
                _particleManager.TriggerEffect(effectPos, EffectType.ExplosionEffect, 5);

                if (structure is ITargetable)
                {
                    structure.Body.Dispose();
                    foreach (var kvp in _structures)
                    {
                        if (kvp is Turret)
                        {

                            ((Turret)kvp).PotentialTargets.Remove(ID);


                        }


                    }
                    _targetManager.DeRegisterObject(structure);

                    if (structure is ITeamable)
                    {
                        _teamManager.DeRegisterObject((ITeamable)structure);
                    }
                }

                _structures.Remove(structure);
            }
        }
    }
}