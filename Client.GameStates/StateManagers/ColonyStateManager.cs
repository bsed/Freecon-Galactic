using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Client.Managers.Networking;
using Freecon.Client.GameStates;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.States;
using System.Collections.Generic;
using Core.Networking;
using Freecon.Client.View;
using Freecon.Core.Networking.Models;
using Freecon.Client.View.CefSharp;
using Freecon.Client.ViewModel;
using Freecon.Core.Networking.Models.Messages;
using Server.Managers;

namespace Freecon.Client.Managers.States
{
    public class DrawableColonyStateManager : NetworkedGameState, ISynchronousUpdate, IDrawableGameState, IDraw, IWebGameState<ColonyViewModel, ColonyWebView>
    {    
        protected SpriteBatch _spriteBatch;
        protected MessageService_ToServer _messageService;

        public Camera2D Camera { get; protected set; }

        public ColonyWebView WebView { get; protected set; }
        public IHasGameWebView RawGameWebView => WebView;

        public IEnumerable<IDraw> DrawList { get { return _drawList; } }
        protected HashSet<IDraw> _drawList;

        public DrawableColonyStateManager(MessageHandlerID messageHandlerId,
            GlobalGameUISingleton globalGameUiSingleton,
            LidgrenNetworkingService networkingService,
            IClientPlayerInfoManager clientPlayerInfoManager,
            SpriteBatch spriteBatch,
            MessageService_ToServer messageService,
            GameWindow gameWindow
            )
            : base(messageHandlerId, clientPlayerInfoManager, networkingService, messageService, GameStateType.Colony)
        {
            _drawList = new HashSet<IDraw>();
            _spriteBatch = spriteBatch;
            Camera = new Camera2D(gameWindow);
            WebView = new ColonyWebView(globalGameUiSingleton, null);

            _synchronousUpdateList.Add(this);
            _messageService = messageService;
            _drawList.Add(this);
            networkingService.RegisterMessageHandler(this, DrawableColonyStateManager_ProcessMessage);       
        }

        void DrawableColonyStateManager_ProcessMessage(object sender, NetworkMessageContainer message)
        {
            switch(message.MessageType)
            {
                case MessageTypes.ClientLoginSuccess:
                {
                    var data = message.MessageData as MessageClientLogin;
                    _clientPlayerInfoManager.PlayerID = data.PlayerInfo.PlayerID;
                    _clientPlayerInfoManager.ActiveShipID = data.PlayerInfo.ActiveShipID;

                    break;
                }
            }
        }
        
        private void LeaveToPlanet()
        {
            _messageService.SendLeaveToPlanetRequest((int)_clientPlayerInfoManager.ActiveShipID);
        }

        private void LeaveToSpace()
        {
            _messageService.SendLeaveToSpaceRequest((int)_clientPlayerInfoManager.ActiveShipID);
        }
       
        public void Update(IGameTimeService gameTime)
        {
            base.Update(gameTime);
#if DEBUG
            if (KeyboardManager.LeaveToPlanet.IsBindTapped())
            {
                ConsoleManager.WriteLine("Sending leave to planet request...", ConsoleMessageType.Notification);
                LeaveToPlanet();

            }
            else if (KeyboardManager.LeaveToSpace.IsBindTapped())
            {
                ConsoleManager.WriteLine("Sending leave to space request...", ConsoleMessageType.Notification);
                LeaveToSpace();

            }
#endif   
        }

        public void Draw(Camera2D camera)
        {
           Debugging.textDrawingService.DrawTextAtLocation(new Vector2(100, 100), "You are in the colony state. Press p to leave to planet. Press s to leave to space. Or don't. Whatever.");
        }

        public void StateWillDraw(Camera2D camera)
        {

        }

        public void StateDidDraw(Camera2D camera)
        {

        }
    }
}