using FarseerPhysics;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.Services;
using Freecon.Client.ViewModel;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Mathematics;
using Freecon.Client.Extensions;
using Freecon.Client.Objects.Pilots;
using Freecon.Client.Objects.Structures;
using System;
using Freecon.Client.Core.Managers;
using System.Linq;
using Client.View.JSMarshalling;
using Freecon.Client.Managers.GUI;
using Freecon.Client.View.CefSharp;
using Server.Managers;

namespace Freecon.Client.View.Xna
{
    public class PlanetGameView : ISynchronousUpdate, IDraw
    {
        public Camera2D Camera { get; set; }
        public PlanetViewModel ViewModel { get; private set; }

        protected ParticleManager _particleManager;
        protected SpriteBatch _spriteBatch;
        protected TextureManager _textureManager;
        protected ProjectileManager _projectileManager;
        protected ClientShipManager _clientShipManager;
        protected UIConversionService _uiConversionService;
        protected Action<JSMarshallContainer> _sendStructurePlacementRequest;
        protected FloatyAreaObjectManager _floatyAreaObjectManager;
        protected GlobalGameUI _globalGameUI;
      

        public PlanetGameView(
            Camera2D camera,
            UIConversionService uiConversionService,
            ParticleManager particleManager,
            PlanetViewModel planetViewModel,
            ProjectileManager projectileManager,
            ClientShipManager clientShipManager,
            SpriteBatch spriteBatch,
            TextureManager textureManager,
            FloatyAreaObjectManager floatyAreaObjectManager,
            GlobalGameUI globalGameUi,
            Action<JSMarshallContainer> sendStructurePlacementRequest)
        {
            ViewModel = planetViewModel;

            _particleManager = particleManager;
            _projectileManager = projectileManager;
            _uiConversionService = uiConversionService;
            _clientShipManager = clientShipManager;
            _spriteBatch = spriteBatch;
            _textureManager = textureManager;
            _floatyAreaObjectManager = floatyAreaObjectManager;
            _globalGameUI = globalGameUi;

            _sendStructurePlacementRequest = sendStructurePlacementRequest;

            globalGameUi.RegisterCallbackVoid("ChangeZoom", ChangeZoom);
            Camera = camera;
            Camera.Zoom = 1f;
        }

        public void Update(IGameTimeService gameTime)
        {
            Camera.Pos = ConvertUnits.ToDisplayUnits(_clientShipManager.PlayerShip.Position);

            if (MouseManager.ScrolledUp || GamepadManager.ZoomIn.IsBindTapped())
            {
                ConsoleManager.WriteLine("Warning: Implement UI Zoom using ChangeZoom container.");
                Camera.Zoom += .1f;
                MathHelper.Clamp(Camera.Zoom, 0.1f, 1f);
            }

            if (MouseManager.ScrolledDown || GamepadManager.ZoomOut.IsBindTapped())
            {
                ConsoleManager.WriteLine("Warning: Implement UI Zoom using ChangeZoom container.");
                Camera.Zoom -= .1f;
                MathHelper.Clamp(Camera.Zoom, 0.1f, 1f);
            }

            

            if (ViewModel.ColonizeMode)
            {
                if (MouseManager.RightMouseClicked)
                {
                    ViewModel.ColonizeMode = false;
                }

                // Temporary, send colonize request
                if (MouseManager.LeftMouseClicked)
                {
                    Vector2 buildingPos = _uiConversionService.MousePosToSimUnits();
                    _sendStructurePlacementRequest(new StructurePlacementRequest {StructureType = StructureTypes.CommandCenter, PosX = buildingPos.X, PosY = buildingPos.Y});
                }
            }

            if (ViewModel.StructurePlacementMode)
            {
                // Cancel with right click
                if (MouseManager.RightMouseClicked)
                {
                    ViewModel.StructurePlacementMode = false;
                }

                // Temporary, send turret placement request
                if (MouseManager.LeftMouseClicked)
                {
                    Vector2 buildingPos = _uiConversionService.MousePosToSimUnits();

                    _sendStructurePlacementRequest(new StructurePlacementRequest {StructureType = StructureTypes.LaserTurret, PosX = buildingPos.X, PosY = buildingPos.Y });
                }
            }
        }

        public void Draw(Camera2D camera)
        {

            //if (SettingsManager.isBloom)
            //_bloom.CheckBackbufferSize(_spriteBatch);
            //_bloom.BeginDraw(_spriteBatch);
            ////_planetStateManager.Draw(camera);
            //_bloom.Draw(_spriteBatch);
            //_selectionManager.Draw(camera);

            // Draw the tiles efficiently, with the camera viewport.
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                null,
                null,
                Camera.GetTransformation(_spriteBatch.GraphicsDevice));

            ViewModel.Level.DrawTiles(Camera);

            _spriteBatch.End();

            // Draw everything else, with the camera viewport.
            _spriteBatch.Begin(
                SpriteSortMode.BackToFront, BlendState.Additive,
                null,
                null,
                null,
                null,
                Camera.GetTransformation(_spriteBatch.GraphicsDevice));

            _particleManager.Draw(camera);

            _spriteBatch.End();

            _spriteBatch.Begin(
                SpriteSortMode.FrontToBack, BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp,
                null,
                null,
                null,
                Camera.GetTransformation(_spriteBatch.GraphicsDevice));

            foreach (var s in ViewModel.Structures)
            {
                switch (s.StructureType)
                {
                    case StructureTypes.LaserTurret:
                        ((Turret)s).Draw(camera);
                        break;

                    default:
                        s.Draw(camera);
                        break;
                }
            }

            Debugging.textDrawingService.DrawTextToScreenRight(25, "Projectiles: " + _projectileManager.GetProjectileCount());
            Debugging.textDrawingService.DrawTextToScreenRight(26, "Structures: " + ViewModel.Structures.Count());
            Debugging.textDrawingService.DrawTextToScreenRight(27, "Latency: " + (MainNetworkingManager.LastLatency * 1000) + "ms");
            Debugging.textDrawingService.DrawTextToScreenRight(28, "Player Speed: " + Debugging.playerShipManager.PlayerShip.LinearVelocity.Length());
            Debugging.textDrawingService.DrawTextToScreenRight(29, "Player Location: " + Debugging.playerShipManager.PlayerShip.Position.ToString());

            _projectileManager.Draw(camera);

            _clientShipManager.Draw(camera);
            _drawShipNames();
            _floatyAreaObjectManager.Draw(camera);

            // Temporary
            if (ViewModel.ColonizeMode)
            {
                var mousePosition = _uiConversionService.PlanetMousePositionToDisplayUnits(
                                        _textureManager.Ground.Width,
                                        _textureManager.Ground.Height);

                _spriteBatch.Draw(_textureManager.Penguin, mousePosition, Color.White);
            }

            _spriteBatch.End();

            // Draw some debug text, relative to the screen's pixels.
            _spriteBatch.Begin();

#if ADMIN
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 8, "Bullets: " + GetProjectileCount());
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 9, "FPS: " + Debugging.textDrawingService.getFPS());
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 25, "Bullets: " + _projectileManager.GetProjectileCount());
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 26, "Structures: " + _structures.Count);
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 27, "Latency: " + (MainNetworkingManager.LastLatency * 1000) + "ms");
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 28, "NPCs: " + Debugging.SimulationManager.GetSimulatedShips().Count);
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 29, "Player Speed: " + LegacyStatics.playerShipManager.PlayerShip.LinearVelocity.Length());
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 30, "Player Location: " + LegacyStatics.playerShipManager.PlayerShip.Position.ToString());
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 31, "Mouse Location: " + MousePosToSimUnits(_clientShipManager.PlayerShip, _spriteBatch));
            Debugging.textDrawingService.DrawTextToScreenRight(_spriteBatch, 32, "Bodies: " + _physicsManager.World.BodyList.Count);
#endif

            _spriteBatch.End();
        }

        /// <summary>
        /// This probably doesn't belong here, but there's not convenient way to get the Camera to the ClientShipManager currently
        /// </summary>
        protected void _drawShipNames()
        {
            foreach (var kvp in _clientShipManager.GetAllShips())
            {
                
                //TODO: This is temporary and will get expensive fast, need to add a .IsAlliedWithPlayerShip bool which is set infrequently
                Color drawColor;
                if (kvp.OnSameTeam(_clientShipManager.PlayerShip))
                    drawColor = Color.Lime;
                else
                    drawColor = Color.Red;


                //Player Names
                if (!(kvp.Pilot is PlayerPilot) && kvp.CurrentHealth > 0) //If the pilot is not the player
                {
                    Vector2 PositionToRender = ConvertUnits.ToDisplayUnits(kvp.Position + Camera.Pos);

                    //PositionToRender.Y += Math.Max(5 + (kvp.Value.currentDrawTex.Height / 3f),
                    //                               Math.Abs(20 + (kvp.Value.currentDrawTex.Height / 1.6f)));

                    //_textDrawingService.DrawTextAtLocationCentered(_spriteBatch, PositionToRender,
                    //                                            kvp.Value.playerName, 1, drawColor);
#if DEBUG
         
                    Debugging.textDrawingService.DrawTextAtLocation(PositionToRender, kvp.playerName);
#endif

                }
            }


        }

        public void DebugDraw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            _clientShipManager.Debug(spriteBatch);

            spriteBatch.End();
        }

        protected void ChangeZoom(JSMarshallContainer container)
        {
            var data = container as ChangeZoom;
            if (data.ZoomDirection == ZoomDirection.In)
            {
                Camera.Zoom += data.Amount;
            }
            else
            {
                Camera.Zoom -= data.Amount;
            }


            MathHelper.Clamp(Camera.Zoom, 0.1f, 1f);

        }
    }
}
