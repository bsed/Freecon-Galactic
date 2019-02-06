using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using Client.View.JSMarshalling;
using Client.View.JSMarshalling.ToClient;
using Freecon.Client.Core.Managers;
using Freecon.Client.Managers.GUI;
using Freecon.Client.Mathematics;
using Freecon.Client.View.CefSharp.States;
using Freecon.Core.Utils;

namespace Freecon.Client.View.CefSharp
{
    public class GameInterfaceWebView : WebViewBase<GameInterfaceViewModel>
    {
        protected readonly NewChatManager _chatManager;

        public GameInterfaceWebView(
            GlobalGameUISingleton gameUserInterface,
            NewChatManager chatManager,
            GameInterfaceViewModel gameInterfaceViewModel
            ) : base(gameUserInterface, gameInterfaceViewModel)
        {
            _chatManager = chatManager;
            
            _chatManager.ChatAdded += (sender, args) =>
            {
                AddChat(args.ChatJson);
            };
        }

        public void Load()
        {
            _gameUserInterface.Load();

            WebLayer.RegisterCallbackVoid("ChatEntered", ChatEntered);

            WebLayer.RegisterCallbackVoid("FetchChats", FetchChats);

            FetchChats(null);
        }

        public void Unload()
        {
            
        }

        private void ChatEntered(JSMarshallContainer container)
        {
            var data = new ChatEntered(container);
            _chatManager.SendChatToServer(data.Chat);

            ClearChatInput();
        }

        private void FetchChats(JSMarshallContainer container)
        {
            ClearChatInput();
            SetChatHistory(_chatManager.GetChatsJson());
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

        public void SetChatHistory(string chats)
        {
            WebLayer.CallJavascriptFunctionAsync("UpdateChatbox", chats);
        }

        public void SwitchActiveState(UIStateManagerContainer container)
        {
            if (!WebLayer.IsLoaded)
            {
                return;
            }

            var stateJson = LowercaseContractResolver.SerializeObject(container);
            WebLayer.CallJavascriptFunctionAsync("SwitchActiveState", stateJson);
        }

        public override void Register()
        {
        }

        public override void Unregister()
        {
        }
        
        public override void Update(IGameTimeService gameTime)
        {
            ViewModel.Update(gameTime);

            _gameUserInterface.Update(gameTime);

            if (!IsLoaded)
            {
                return;
            }

            var interfaceJson = LowercaseContractResolver.SerializeObject(ViewModel.CurrentGlobalGameInterfaceState);

            WebLayer.CallJavascriptFunctionAsync(
                "UpdateGameInterface",
                interfaceJson
            );
        }

        public override void Draw(Camera2D camera)
        {
            _gameUserInterface.Draw(camera);
        }
    }
}
