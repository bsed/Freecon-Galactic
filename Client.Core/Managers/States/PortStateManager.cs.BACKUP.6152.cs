using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SRClient.GUI;
using SRClient.Objects;
using MemBus;
using SRClient.BusMessages;
using SRClient.Factories;
using Freecon.Models.TypeEnums;
using Core.Models;
using Core.Models.CargoHandlers;

namespace SRClient.Managers.States
{
    public class PortStateManager : GameState
    {
        private IBus _bus;
        private ChatManager _chatManager;
        private HudElementFactoryManager _hudElementFactory;
        private TextureManager _textureManager;

        #region Textures

        private ContentManager Content;
        private Texture2D tex_Interface;

        #endregion


        public CargoHandlerPort_ROVM Cargo { get; set; }

        public Dictionary<int, PortShip> ShipsInPort = new Dictionary<int, PortShip>(5);
                                                   //Ships currently docked in port

        // Gamestate
        private GameStates state;
        // UI Elements
        public List<BaseUI> Windows;

        public PortStateManager(IBus bus,
                                ChatManager chatManager, 
                                HudElementFactoryManager hudElementManager, 
                                TextureManager textureManager):base(null, null, null, null)
        {
            _bus = bus;
            _chatManager = chatManager;
            _hudElementFactory = hudElementManager;
            _textureManager = textureManager;

            Windows = new List<BaseUI>();
            state = GameStates.loading;
            tex_Interface = _textureManager.Port_StationHD;
            Windows.Add(_hudElementFactory.CreatePortWindowButtons(Windows));
            Windows.Add(_hudElementFactory.CreateAuxiliaryInformationPanels(Windows));
            state = GameStates.updating;
        }

        public void SetState()
        {
            _chatManager.renderChat = true;

            _bus.Publish(new MSetWindowListMessage(Windows));
            //_hudElementFactory.SetWindowList(Windows);
        }

        public void Reset()
        {
        }

        public void Update(GameTime gameTime)
        {
            switch (state)
            {
                case GameStates.transitional:
                    _bus.Publish(new MChangeStateMessage(GameStates.Space));
                    break;
            }
        }

        public void Clear()
        {
            ShipsInPort.Clear();

            if (Windows != null)
            {
                for (int w = 0; w < Windows.Count; w++)
                {
                    if (Windows[w].Name == "DropDownWindow")
                    {
                        Windows.RemoveAt(w);
                        w--;
                    }
                    else if (Windows[w].Name == "Port Interface")
                    {
                        Windows.RemoveAt(w);
                        w--;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float DrawScale = spriteBatch.GraphicsDevice.Viewport.Width/(float) tex_Interface.Width;

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Draw(tex_Interface, Vector2.Zero, null, Color.White, 0, Vector2.Zero, DrawScale,
                             SpriteEffects.None, 1f);
            spriteBatch.End();
        }
    }

    /// <summary>
    /// Used for displaying ships currently docked in port
    /// </summary>
    public class PortShip
    {
        private string playerName;
        private int shipID;

        public PortShip(int shipID, string playerName)
        {
            this.shipID = shipID;
            this.playerName = playerName;
        }
    }

}