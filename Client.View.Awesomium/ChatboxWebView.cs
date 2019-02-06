using Awesomium.Core;
using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework; using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using Client.Core.Managers;

namespace Freecon.Client.View.Awesomium
{
    public class ChatboxWebView : AwesomiumWebView<ChatboxWebLayer, ChatboxModel>
    {
        protected readonly NewChatManager _chatManager;

        Action<string> _chatEntered;

        public ChatboxWebView(
            ChatboxWebLayer webLayer,
            ChatboxModel chatboxViewModel,
            NewChatManager chatManager,
            SpriteBatch spriteBatch,
            Action<string> chatEntered
        ) : base(webLayer, chatboxViewModel, spriteBatch)
        {
            _chatManager = chatManager;

            _chatEntered = chatEntered;

            WebLayer.RegisterCallback("ChatEntered", ChatEntered);
        }

        private JSValue ChatEntered(object arguments, JavascriptMethodEventArgs jsEvent)
        {
            _chatManager.SendChatToServer((string)jsEvent.Arguments[0]);

            return default(JSValue);
        }

        /// <summary>
        /// Adds a chat.
        /// C# wrapper around web code.
        /// </summary>
        /// <param name="chatJson">The chat json.</param>
        public void AddChat(string chatJson)
        {
            WebLayer.CallJavascriptFunctionAsync("AddChat", chatJson);
        }

        /// <summary>
        /// Clears the current user input.
        /// C# wrapper around web code.
        /// </summary>
        public void ClearChatInput()
        {
            WebLayer.CallJavascriptFunctionAsync("ClearChatInput");
        }

        /// <summary>
        /// Calculates the position.
        /// For the chatbox, keeps it mounted to the bottom left.
        /// </summary>
        /// <returns>Return the calculated position.</returns>
        public Point CalculatePosition()
        {
            if (WebLayer.WebTexture == null)
            {
                return new Point(0, 0);
            }

            var offsetY = _spriteBatch.GraphicsDevice.Viewport.Height - WebLayer.WebTexture.Height;

            var p = new Point(0, offsetY);

            WebLayer.Position = p;

            return p;
        }

        public override void Update(IGameTimeService gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(IGameTimeService gameTime)
        {
            CalculatePosition();

            base.Draw(gameTime);
        }
    }
}
