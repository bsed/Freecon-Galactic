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

        public Dictionary<byte, Good> GoodsForSale = new Dictionary<byte, Good>(5); //Goods the port is selling

        public Dictionary<byte, Good> GoodsForPurchase = new Dictionary<byte, Good>(5);
                                             //Goods the port is purchasing

        public Dictionary<byte, Service> OutfitForSale = new Dictionary<byte, Service>(5);
                                                //Services the port is selling

        public List<ShipGood> ShipsForSale = new List<ShipGood>(5); //Ships the port is selling

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
            GoodsForSale.Clear();
            GoodsForPurchase.Clear();
            OutfitForSale.Clear();
            ShipsInPort.Clear();
            ShipsForSale.Clear();
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

    #region Services

    /// <summary>
    /// Base class for services provided at ports (such as ship repair)
    /// </summary>
    public class Service
    {
        public int currentPrice;
        public string name;
        public Port port;
        public byte serviceType;

        public Service(byte serviceType, string name, int currentPrice)
        {
            this.serviceType = serviceType;
            this.name = name;
            this.currentPrice = currentPrice;
        }

        /// <summary>
        /// Does everything necessary to the player ship, depending on the service purchased
        /// </summary>
        public virtual void doService()
        {
        }
    }

    public class HullRepair : Service
    {
        public HullRepair(byte serviceType, string name, int currentPrice)
            : base(serviceType, name, currentPrice)
        {
            name = "Hull Repair";
            serviceType = (byte) OutfitTypes.hullRepair;
        }
    }

    #endregion

    #region Goods

    /// <summary>
    /// Base class for goods that can be bought and sold at ports
    /// </summary>
    public class Good
    {
        public int cargoSpaceTaken = 1;
        public int currentPrice;
        public string description;
        public GoodTypes goodType;
        public string name;
        public int numInStock;
        public Port port;

        public Good(GoodTypes goodType, string name, int currentPrice, int numInStock)
        {
            this.goodType = goodType;
            this.name = name;
            this.currentPrice = currentPrice;
            this.numInStock = numInStock;
        }
    }


    /// <summary>
    /// Will have to make a separate good for each type of ship sold later
    /// We could always just inherit from ShipGood too
    /// </summary>
    public class ShipGood : Good
    {
        /// <summary>
        /// ID used for purchase requests
        /// </summary>
        public Int16 ID;

        /// <summary>
        /// Cargo that the ship can hold
        /// </summary>
        public Int16 cargo;

        /// <summary>
        /// Energy that the ship can max at
        /// </summary>
        public Int16 energy;

        /// <summary>
        /// Hull that the ship has max
        /// </summary>
        public Int16 hull;

        /// <summary>
        /// Shields that the ship has max
        /// </summary>
        public Int16 shields;

        /// <summary>
        /// Type of ship. 
        /// </summary>
        public ShipTypes shipType;

        /// <summary>
        /// Pretty much a slider saying if this is cheap or not... 
        /// Perhaps based on galactic price, or just based relative to all of the other ships in the game.
        /// </summary>
        public byte value;

        /// <summary>
        /// Ship that is sold in a port.
        /// </summary>
        /// <param name="p">Port that ship is sold at</param>
        /// <param name="shipType"></param>
        /// <param name="currentPrice"></param>
        /// <param name="numInStock"></param>
        /// <param name="cargoSpaceTaken"></param>
        public ShipGood(ShipTypes shipType, string name, string description, Int16 ID, int currentPrice,
                        int numInStock, Int16 shields, Int16 hull, Int16 energy, byte value, Int16 cargo)
            : base(GoodTypes.Ship, name, currentPrice, numInStock) // Not sure about inception ships
        {
            this.shipType = shipType;
            this.description = description;
            this.ID = ID;
            this.shields = shields;
            this.hull = hull;
            this.energy = energy;
            this.value = value;
            this.cargo = cargo;
        }
    }

    public class Woman : Good
    {
        public Woman(GoodTypes goodType, string name, int currentPrice, int numInStock)
            : base(goodType, name, currentPrice, numInStock)
        {
            goodType = GoodTypes.Woman;
            cargoSpaceTaken = 10;
        }
    }

    #endregion
}