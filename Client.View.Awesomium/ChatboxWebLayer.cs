using AwesomiumUiLib;
using Microsoft.Xna.Framework; using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.View.Awesomium
{
    public class ChatboxWebLayer : AwesomiumUI
    {
        public bool Loaded = false;

        public ChatboxWebLayer(GameWindow gameWindow, SpriteBatch spriteBatch)
            : base(gameWindow, spriteBatch)
        {
            LoadCompleted += (sender, e) =>
            {
                Loaded = true;
            };
        }

        public override void Update(IGameTimeService gameTime)
        {
            base.Update(gameTime);
        }
    }
}
