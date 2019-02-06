using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Freecon.Client.Managers.GUI;
using Freecon.Client.Mathematics;
using Freecon.Core.Configs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.View.CefSharp
{
    public class GlobalGameUISingleton : IGlobalGameUISingleton
    {
        protected bool _isLoaded;

        protected IClientWebViewConfig _config;
        protected SpriteBatch _spriteBatch;
        protected GraphicsDevice _graphicsDevice;
        protected GameWindow _gameWindow;

        public GlobalGameUI GameUI { get; protected set; }

        public GlobalGameUISingleton(
            IClientWebViewConfig config,
            GraphicsDevice graphicsDevice,
            GameWindow window,
            SpriteBatch spriteBatch)
        {
            _config = config;
            _graphicsDevice = graphicsDevice;
            _gameWindow = window;
            _spriteBatch = spriteBatch;
            GameUI = new GlobalGameUI();
        }

        public void Load()
        {
            if (_isLoaded)
            {
                Unload();
            }

            var UITexture = new Texture2D(_graphicsDevice, _gameWindow.ClientBounds.Width, _gameWindow.ClientBounds.Height, true, SurfaceFormat.Color);

            GameUI.Load(_gameWindow.Handle, UITexture, Point.Zero, _config.GlobalGameInterface.Url);
            _gameWindow.ClientSizeChanged += _gameWindow_ClientSizeChanged;
            _isLoaded = true;
        }

        public void Unload()
        {
            _gameWindow.ClientSizeChanged -= _gameWindow_ClientSizeChanged;

            Unregister();

            GameUI.ShutDown();
            GameUI = null;
            _isLoaded = false;
        }

        public void Register()
        {
        }

        public void Unregister()
        {
            
        }

        public virtual void Update(IGameTimeService gameTime)
        {
            GameUI.Update(gameTime);
        }

        public virtual void Draw(Camera2D camera)
        {
            GameUI.UpdateBitmap(this._graphicsDevice);

            if (GameUI.UITexture == null)
                return;
            _spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            _spriteBatch.Draw(GameUI.UITexture, Vector2.Zero, Color.White);

            _spriteBatch.End();
        }

        private void _gameWindow_ClientSizeChanged(object sender, EventArgs e)
        {
            if (_gameWindow.ClientBounds.Height == 0 || _gameWindow.ClientBounds.Width == 0)
                return;

            var UITexture = new Texture2D(
                _graphicsDevice,
                _gameWindow.ClientBounds.Width,
                _gameWindow.ClientBounds.Height,
                true,
                SurfaceFormat.Color
            );
            GameUI.Resize(UITexture);
        }
    }
}
