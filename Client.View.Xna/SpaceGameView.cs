using FarseerPhysics;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.Services;
using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Space;
using Freecon.Client.Mathematics;
using Freecon.Client.Mathematics.Effects;
using Freecon.Client.Objects.Pilots;
using System;
using System.Linq;
using Freecon.Client.Extensions;
using Freecon.Client.Core.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects;
using Client.View.JSMarshalling;
using Freecon.Client.Managers.GUI;
using Freecon.Client.View.CefSharp;
using Server.Managers;

namespace Freecon.Client.View.Xna
{
    public class SpaceGameView : ISynchronousUpdate, IDraw
    {
        protected BackgroundManager _background;
        protected BloomComponent _bloom;
        protected ParticleManager _particleManager;
        protected ProjectileManager _projectileManager;
        protected ClientShipManager _clientShipManager;
        protected SpaceObjectManager _spaceObjectManager;
        protected SpriteBatch _spriteBatch;
        protected UIConversionService _uiConversionService;
        protected SpaceManager _spaceManager;
        protected SpaceViewModel _spaceViewModel;
        protected FloatyAreaObjectManager _floatyAreaObjectManager;
        protected MessageService_ToServer _messageService;

        protected Vector2 _oldPosition;

        public bool StructurePlacementModeEnabled { get { return _spaceViewModel.StructurePlacementModeEnabled; } set { _spaceViewModel.StructurePlacementModeEnabled = value; } }

        public Camera2D Camera { get; set; }

        public SpaceGameView(
            MessageService_ToServer messageService,
            BackgroundManager background,
            BloomComponent bloom,
            Camera2D camera,
            ParticleManager particleManager,
            ProjectileManager projectileManager,
            ClientShipManager clientShipManager,
            SpaceManager spaceManager,
            SpaceObjectManager spaceObjectManager,
            SpriteBatch spriteBatch,
            UIConversionService uiConversionService,
            FloatyAreaObjectManager floatyAreaObjectManager,
            SpaceViewModel viewModel,
            GlobalGameUI globalGameUi
            )
        {
            _background = background;
            _bloom = bloom;
            _particleManager = particleManager;
            _projectileManager = projectileManager;
            _clientShipManager = clientShipManager;
            _spaceObjectManager = spaceObjectManager;
            _spaceManager = spaceManager;
            _spriteBatch = spriteBatch;
            _uiConversionService = uiConversionService;
            _spaceViewModel = viewModel;
            _floatyAreaObjectManager = floatyAreaObjectManager;
            _messageService = messageService;


            globalGameUi.RegisterCallbackVoid("ChangeZoom", ChangeZoom);

            Camera = camera;
            Camera.Zoom = 1f;
        }

        public void Update(IGameTimeService gameTime)
        {
            Camera.Pos = ConvertUnits.ToDisplayUnits(_clientShipManager.PlayerShip.Position);
            Camera.UpdateCameraShake();

            // Todo: See if zooming in space is broken, if it is, then finish refactoring RotationalCamera away.
            //Temporary fix for now, it was broken.
            if (MouseManager.ScrolledUp || GamepadManager.ZoomIn.IsBindTapped())
            {
                ConsoleManager.WriteLine("Warning: Implement UI Zoom using ChangeZoom container.");
                Camera.Zoom += .1f;
            }

            if (MouseManager.ScrolledDown || GamepadManager.ZoomOut.IsBindTapped())
            {
                ConsoleManager.WriteLine("Warning: Implement UI Zoom using ChangeZoom container.");
                Camera.Zoom -= .1f;
            }


            if (_spaceViewModel.StructurePlacementModeEnabled)
            {
                if (MouseManager.RightButtonPressed)
                {
                    // Cancel with right click
                    _spaceViewModel.StructurePlacementModeEnabled = false;
                }

              
            }

            if (_clientShipManager.PlayerShip != null)
            {
                var shipDifference = _oldPosition - ConvertUnits.ToDisplayUnits(_clientShipManager.PlayerShip.Position);

                _background.Update(_clientShipManager.PlayerShip.Position, shipDifference);

                _spaceObjectManager.Update(gameTime, _clientShipManager.PlayerShip != null && _clientShipManager.PlayerShip.EnterMode);

                _oldPosition = ConvertUnits.ToDisplayUnits(_clientShipManager.PlayerShip.Position);
            }
        }

        public void Draw(Camera2D camera)
        {


            Debugging.textDrawingService.DrawTextToScreenRight(25, "Projectiles: " + _projectileManager.GetProjectileCount());
            Debugging.textDrawingService.DrawTextToScreenRight(26, "Structures: " + _spaceViewModel.Structures.Count());
            Debugging.textDrawingService.DrawTextToScreenRight(27, "Latency: " + (MainNetworkingManager.LastLatency * 1000) + "ms");
            Debugging.textDrawingService.DrawTextToScreenRight(28, "Player Speed: " + Debugging.playerShipManager.PlayerShip.LinearVelocity.Length());
            Debugging.textDrawingService.DrawTextToScreenRight(29, "Player Location: " + Debugging.playerShipManager.PlayerShip.Position.ToString());


            //Vector2 mousePoss = _uiConversionService.MousePosToSimUnits();
            //float shipToMousAngle = AIHelper.GetAngleToPosition(_clientShipManager.PlayerShip.Position, mousePoss) * 180 / (float)Math.PI;
            //float shipAngle = AIHelper.ClampRotation(_clientShipManager.PlayerShip.Rotated) * 180 / (float)Math.PI;
            //float angleToRotate = AIHelper.GetRotationToPosition(_clientShipManager.PlayerShip.Position, mousePoss, _clientShipManager.PlayerShip.Rotated) * 180 / (float)Math.PI;
            //Debugging.textDrawingService.DrawTextToScreenRight(31, "Angle, Ship To Mouse: " + shipToMousAngle);
            //Debugging.textDrawingService.DrawTextToScreenRight(32, "Ship Angle, clamped: " + shipAngle);
            //Debugging.textDrawingService.DrawTextToScreenRight(33, "Angle to rotate: " + angleToRotate);
            //Debugging.textDrawingService.DrawTextToScreenRight(34, "Mouse Position: " + mousePoss);
         

            //_bloom.CheckBackbufferSize(_spriteBatch);
            //_spaceStateManager.DrawBloom(gameTime, _bloom, SettingsManager.isBloom);
            //_selectionManager.Draw(camera);

            // Sometimes there is a discrepency due to methods calling world update
            Camera.Pos = ConvertUnits.ToDisplayUnits(_clientShipManager.PlayerShip.Position);

            if (SettingsManager.isBloom)
            {
                _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                _spriteBatch.Draw(_bloom.screenTarget, Vector2.Zero, Color.White);

                _spriteBatch.End();
                               

            }

            DrawBloom(camera, _bloom, true);
        }

        /// <summary>
        /// This probably doesn't belong here, but there's not convenient way to get the Camera to the ClientShipManager currently
        /// </summary>
        protected void _drawShipNames()
        {
            foreach (var kvp in _clientShipManager.GetAllShips())
            {
                // TODO: This is temporary and will get expensive fast, need to add a .IsAlliedWithPlayerShip bool which is set infrequently
                Color drawColor;
                if (kvp.OnSameTeam(_clientShipManager.PlayerShip))
                    drawColor = Color.Lime;
                else
                    drawColor = Color.Red;


                // Player Names
                if (!(kvp.Pilot is PlayerPilot) && kvp.CurrentHealth > 0) //If the pilot is not the player
                {
                    Vector2 PositionToRender = _uiConversionService.SimPosToScreenUnits(kvp.Position);


                    if(kvp is Ship3D)
                    {
                        //I don't fucking know I'll fix it later
                        PositionToRender.Y += Math.Max(5 + 2 / 3f,
                        Math.Abs(20 + (2 / 1.4f))) * Camera.Zoom;
                    }
                    else
                    {
                        PositionToRender.Y += Math.Max(5 + (kvp.currentDrawTex.Height / 3f),
                        Math.Abs(20 + (kvp.currentDrawTex.Height / 1.4f))) * Camera.Zoom;

                    }

                    //_textDrawingService.DrawTextAtLocationCentered(_spriteBatch, PositionToRender,
                    //                                            kvp.playerName, 1, drawColor);
#if DEBUG
                    Debugging.textDrawingService.DrawTextAtLocationCentered(PositionToRender, kvp.playerName,
                        Camera.Zoom, drawColor);
#endif
                }
            }
        }

        public void DrawBloom(Camera2D camera, BloomComponent bloom, bool isBloom)
        {
            _background.Draw(false, Camera.Pos);

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                null,
                null,
                null,
                null,
                Camera.GetTransformation(_spriteBatch.GraphicsDevice));

            _spaceManager.Draw(_spriteBatch, Camera.Pos, Camera.Zoom);

            
            _spriteBatch.End();

            // Draw particles in different spritebatch. Otherwise they look absolutely awful.
            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive,
                null,
                null,
                null,
                null,
                Camera.GetTransformation(_spriteBatch.GraphicsDevice));

            _particleManager.Draw(camera);

            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                null,
                null,
                null,
                null,
                Camera.GetTransformation(_spriteBatch.GraphicsDevice));

            foreach (var s in _spaceViewModel.Structures)
            {
                switch (s.StructureType)
                {
                    default:
                        s.Draw(camera);
                        break;
                }
            }

            _floatyAreaObjectManager.Draw(_spriteBatch, camera);
            _projectileManager.Draw(camera);
            _clientShipManager.Draw(camera);
            _drawShipNames();

            _spriteBatch.End();
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
            
        }

    }
}
