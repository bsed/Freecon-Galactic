using FarseerPhysics;
using Freecon.Client.Core.Interfaces;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using System;

namespace Freecon.Client.Core.Services
{
    public class UIConversionService : ISynchronousUpdate
    {
        CameraService _cameraService;
        PlayerShipManager _playerShipManager;
        SpriteBatch _spriteBatch;

        public UIConversionService(
            CameraService cameraService,
            PlayerShipManager playerShipManager,
            SpriteBatch spriteBatch)
        {
            _cameraService = cameraService;
            _playerShipManager = playerShipManager;
            _spriteBatch = spriteBatch;
        }

        /// <summary>
        /// Returns the current mouse position in sim units
        /// </summary>
        /// <returns></returns>
        public Vector2 MousePosToSimUnits()
        {
            //Vector2 mousePos = new Vector2();

            //var playerShip = _playerShipManager.PlayerShip;
            //var zoom = _cameraService.CurrentZoom;

            //mousePos.X = playerShip.Position.X * 100 + (MouseManager.CurrentPosition.X - _spriteBatch.GraphicsDevice.Viewport.Width / 2f) / zoom;
            //mousePos.Y = playerShip.Position.Y * 100 + (MouseManager.CurrentPosition.Y - _spriteBatch.GraphicsDevice.Viewport.Height / 2f) / zoom;

            //return mousePos / 100f;

            return ScreenPosToSimUnits(MouseManager.CurrentPosition);
        }

        /// <summary>
        /// Returns the current mouse position in sim units
        /// </summary>
        /// <returns></returns>
        public Vector2 ScreenPosToSimUnits(Vector2 screenPos)
        {
            Vector2 simPos = new Vector2();

            var playerShip = _playerShipManager.PlayerShip;
            var zoom = _cameraService.CurrentZoom;

            simPos.X = playerShip.Position.X * 100 + (screenPos.X - _spriteBatch.GraphicsDevice.Viewport.Width / 2f) / zoom;
            simPos.Y = playerShip.Position.Y * 100 + (screenPos.Y - _spriteBatch.GraphicsDevice.Viewport.Height / 2f) / zoom;

            return simPos / 100f;
        }

        public Vector2 SimPosToScreenUnits(Vector2 simPos)
        {
            Vector2 screenPos = new Vector2();

            var playerShip = _playerShipManager.PlayerShip;
            var zoom = _cameraService.CurrentZoom;

            screenPos.X = (simPos.X * 100 - playerShip.Position.X * 100) * zoom + _spriteBatch.GraphicsDevice.Viewport.Width / 2f;
            screenPos.Y = (simPos.Y * 100 - playerShip.Position.Y * 100) * zoom + _spriteBatch.GraphicsDevice.Viewport.Height / 2f;

            return screenPos;
        }

        /// <summary>
        /// Converts the given mousePos to sim units
        /// </summary>
        /// <returns></returns>
        public Vector2 MousePosToSimUnits(Vector2 mousePos)
        {
            var playerShip = _playerShipManager.PlayerShip;
            var zoom = _cameraService.CurrentZoom;

            mousePos.X = playerShip.Position.X * 100 + (mousePos.X - _spriteBatch.GraphicsDevice.Viewport.Width / 2f) / zoom;
            mousePos.Y = playerShip.Position.Y * 100 + (mousePos.Y - _spriteBatch.GraphicsDevice.Viewport.Height / 2f) / zoom;

            return mousePos / 100f;
        }

        public Vector2 PlanetMousePositionToDisplayUnits(float tileWidth, float tileHeight)
        {
            var playerShip = _playerShipManager.PlayerShip;
            var zoom = _cameraService.CurrentZoom;

            Vector2 mousePos = new Vector2();

            mousePos.X = playerShip.Position.X + (MouseManager.CurrentPosition.X - _spriteBatch.GraphicsDevice.Viewport.Width / 2f) / (tileWidth * zoom);
            mousePos.Y = playerShip.Position.Y + (MouseManager.CurrentPosition.Y - _spriteBatch.GraphicsDevice.Viewport.Height / 2f) / (tileHeight * zoom);

            return ConvertUnits.ToDisplayUnits(mousePos);
        }

        public void Update(IGameTimeService gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
