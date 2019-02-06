using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Core.Objects;
using Freecon.Client.GameStates;
using Freecon.Client.Core.States;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Managers.Networking;
using Freecon.Core.Networking.Models;
using Core.Networking;
using Freecon.Client.Mathematics;
using Freecon.Client.View;
using Freecon.Client.View.CefSharp;
using Freecon.Client.ViewModel;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Client.Managers.States
{
    public class DrawablePortStateManager : NetworkedGameState, ISynchronousUpdate, IDraw, IWebGameState<PortViewModel, PortWebView>
    {
        private SpriteBatch _spriteBatch;

        public PortWebView WebView { get; protected set; }
        public IHasGameWebView RawGameWebView => WebView;

        MessageService_ToServer _messageService;

        private Texture2D tex_Interface;

        // Services the port is selling
        public Dictionary<byte, PortService> OutfitForSale { get; set; }

        // Ships currently docked in port
        public Dictionary<int, PortShip> ShipsInPort { get; set; }


        HashSet<IDraw> _drawList;

        public DrawablePortStateManager(
            MessageHandlerID messageHandlerId,
            IClientPlayerInfoManager clientPlayerInfoManager,
            LidgrenNetworkingService networkingService,
            GlobalGameUISingleton globalGameUiSingleton,
            SpriteBatch spriteBatch,
            MessageService_ToServer messageService
            )
            : base(messageHandlerId, clientPlayerInfoManager, networkingService, messageService, GameStateType.Port)
        {
            _drawList = new HashSet<IDraw>();

            // Todo: Make this class use a ViewModel
            WebView = new PortWebView(globalGameUiSingleton, null);
            _spriteBatch = spriteBatch;

            OutfitForSale = new Dictionary<byte, PortService>(5);

            ShipsInPort = new Dictionary<int, PortShip>(5);
            
            _synchronousUpdateList.Add(this);
            _messageService = messageService;
            _drawList.Add(this);
            networkingService.RegisterMessageHandler(this, DrawablePortStateManager_ProcessMessage);
        }

        private void DrawablePortStateManager_ProcessMessage(object sender, NetworkMessageContainer message)
        {
            switch (message.MessageType)
            {
                case MessageTypes.PortDockApproval:
                {
                    Clear();
                    var data = message.MessageData as PortEntryData;

                    // TODO: Implement reading this data, though much of this is used purely in the UI.

                    break;
                }

                case MessageTypes.ClientLoginSuccess:
                {
                    var data = message.MessageData as MessageClientLogin;
                    if (data == null)
                    {
                        ClientLogger.LogError("Could not deserialize MessageClientLogin: " + message.MessageData);
                        return;
                    }

                    _clientPlayerInfoManager.PlayerID = data.PlayerInfo.PlayerID;
                    _clientPlayerInfoManager.ActiveShipID = data.PlayerInfo.ActiveShipID;

                    break;
                }
            }
        }

        public void Update(IGameTimeService gameTime)
        {
           
        }

        public void Clear()
        {
            OutfitForSale.Clear();
            ShipsInPort.Clear();
        }

        public void Draw(Camera2D camera)
        {
            float DrawScale = _spriteBatch.GraphicsDevice.Viewport.Width / (float) tex_Interface.Width;

            _spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            _spriteBatch.Draw(tex_Interface, Vector2.Zero, null, Color.White, 0, Vector2.Zero, DrawScale,
                              SpriteEffects.None, 1f);
            _spriteBatch.End();
        }
    }

}
