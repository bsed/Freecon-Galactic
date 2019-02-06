using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SRClient.Managers;
using SRClient.Managers.Networking;
using SRClient.Managers.States;
using SRClient.Objects;
using MemBus;
using SRClient.Interfaces;
using SRClient.BusMessages;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models;



namespace SRClient.GUI
{
    partial class HudElementManager : IHandle<MSetWindowListMessage>
    {
        #region Objects

        // Object References
        private IBus _bus;
        private ClientManager _clientManager;
        private ContentManager _content;
        private DebugTextManager _debugTextManager;
        private SpriteBatch _spriteBatch;
        private PortStateManager _portStateManager;
        private TextureManager _textureManager;
        private ShipManager _shipManager;


        // Galaxy HUD Manager
        public Vector2 MoveGalaxyMap;
        public Texture2D blank;

        // Set to a reference when states are switched, 
        // Actions here will affect anything in PortStateManager or SpaceStateManager
        private IList<BaseUI> InternalWindows; // Setters

        // GC Stuff
        private StandardForm tempsf;
        private Button tempbutton;

        // Formatting of Windows
        private int verticalOffset = 22;

        #endregion

        #region Internal Functions

        public HudElementManager(IBus bus,
                                 DebugTextManager dtm,
                                 SpriteBatch spriteBatch, 
                                 ContentManager Content,
                                 ClientManager clientManager,
                                 PortStateManager portStateManager,
                                 TextureManager textureManager, 
                                 ShipManager shipManager)
        {
            _bus = bus;
            _spriteBatch = spriteBatch;
            _content = Content;
            _debugTextManager = dtm;
            _portStateManager = portStateManager;
            _textureManager = textureManager;
            _clientManager = clientManager;
            _shipManager = shipManager;

            blank = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);

            blank.SetData(new[] { Color.White });

            _bus.Subscribe(this);
        }

        #region Create Standard Form

        /// <summary>
        /// Creates a new StandardForm and passes by reference (efficient)
        /// </summary>
        /// <param name="sf">Form to assign value</param>
        /// <param name="name">Name of form</param>
        public void CreateStandardForm(ref StandardForm sf, string name)
        {
            sf = new StandardForm(InternalWindows, _spriteBatch, _content, name);
            sf.Initialize(_spriteBatch, _content); // Important because UI elements store references to both.
        }

        /// <summary>
        /// Creates a new StandardForm and passes by value (not as efficient)
        /// </summary>
        /// <param name="name">Name of form</param>
        /// <returns>Value of form</returns>
        public StandardForm CreateStandardForm(string name)
        {
            var sf = new StandardForm(InternalWindows, _spriteBatch, _content, name);
            sf.Initialize(_spriteBatch, _content); // Important because UI elements store references to both.
            return sf;
        }

        #endregion

        #region Create Close Button

        public Button CreateCloseButton(string name, BaseForm bf)
        {
            var b = new Button(_spriteBatch, _content, name, bf);
            b.Initialize(_spriteBatch, _content);
            b.OnClickEvent += b_OnClickEvent;
            b.Text = "Close";
            return b;
        }

        public Button CreateRightClickCloseButton(string name, BaseForm bf)
        {
            var b = new Button(_spriteBatch, _content, name, bf);
            b.Initialize(_spriteBatch, _content);
            b.OnClickEvent += b_OnClickEvent;
            b.Text = "Close";
            return b;
        }

        private void b_OnClickEvent(BaseUI sender)
        {
            var b = (Button) sender;
            //b.Owner.Close();
            RemoveWindowFromList(b.Owner);
        }

        #endregion

        #region Create Button

        public Button CreateStaticButton(string name, BaseForm bf)
        {
            var staticButton = new Button(_spriteBatch, _content, name, bf);
            staticButton.SetBackgroundMaterial(_textureManager.staticWindowTexture_Off);
            staticButton.IsStatic = true;
            staticButton.OnClickEvent += staticButton_OnClickEvent;
            return staticButton;
        }

        private void staticButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonHeld)
                return;

            var b = (Button) sender;
            b.Trigger = !b.Trigger;
            if (b.Trigger)
            {
                b.Owner.IsStatic = true;
                b.SetBackgroundMaterial(_textureManager.staticWindowTexture_On);
            }
            else
            {
                b.Owner.IsStatic = false;
                b.SetBackgroundMaterial(_textureManager.staticWindowTexture_Off);
            }
        }

        private void staticButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Button) sender;
            b.Location = new Vector2(b.Owner.Location.X, b.Owner.Location.Y);
        }

        #endregion

        #region Set Window List

        public void SetWindowList(IList<BaseUI> listToSet)
        {
            InternalWindows = listToSet;
        }

        #endregion

        #region Remove Window From Window List

        public void RemoveWindowFromList(BaseUI ToRemove)
        {
            for (int i = 0; i < InternalWindows.Count; i++)
            {
                if (ToRemove == InternalWindows[i])
                    InternalWindows.RemoveAt(i);
            }
        }

        #endregion

        #region Create Label

        /// <summary>
        /// Creates a new Label and passes by reference (efficient)
        /// </summary>
        /// <param name="sf">Label to assign value</param>
        /// <param name="name">The text to display</param>
        public void CreateLabel(ref Label lbl, string text)
        {
            lbl = new Label(_spriteBatch, _content, text);
            lbl.Initialize(_spriteBatch, _content);
        }


        /// <summary>
        /// Creates a new Label and passes by value (not as efficient)
        /// </summary>
        /// <param name="name">The text to display</param>
        /// <returns>Value of label</returns>
        public Label CreateLabel(string text)
        {
            var lbl = new Label(_spriteBatch, _content, text);
            lbl.Initialize(_spriteBatch, _content);
            return lbl;
        }

        /// <summary>
        /// Creates a new Label in a StandardForm and passes by value (not as efficient)
        /// </summary>
        /// <param name="sf">Standard Form to link</param>
        /// <param name="name">The text to display</param>
        /// <returns>Value of label</returns>
        public Label CreateLabel(ref StandardForm sf, string text)
        {
            var lbl = new Label(_spriteBatch, _content, text, sf);
            lbl.Initialize(_spriteBatch, _content);
            return lbl;
        }

        #endregion

        #region Create Button

        public Button CreateButton(string name, BaseForm bf)
        {
            var b = new Button(_spriteBatch, _content, name, bf);
            b.Initialize(_spriteBatch, _content);
            return b;
        }

        #endregion

        #region Create Data Button

        public Button CreateDataButton(string name, object data, BaseForm bf)
        {
            var b = new Button(_spriteBatch, _content, name, bf);
            b.data = data;
            b.Initialize(_spriteBatch, _content);
            return b;
        }

        #endregion

        #region Create Bar

        public Bar CreateBar(string name, BaseForm bf)
        {
            var b = new Bar(_spriteBatch, _content, name, bf);
            b.Initialize(_spriteBatch, _content);
            return b;
        }

        #endregion

        #region Drawing and Window list updates

        private GameTime _gameTime;

        public void Update(GameTime gameTime)
        {
            _gameTime = gameTime;

            //sort(0, Windows.Count - 1);
            if (GetWindows == null)
                return;
            for (int i = GetWindows.Count - 1; i >= 0; i--)
            {
                GetWindows[i].Update(gameTime);
            }
            Sort(0, GetWindows.Count - 1);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (GetWindows == null)
                return;
            for (int i = 0; i < GetWindows.Count; i++)
            {
                GetWindows[i].Draw(spriteBatch, _gameTime);
            }
        }

        public void Sort(int left, int right)
        {
            if (right <= 0)
                return;
            int pivot, l_holder, r_holder;

            l_holder = left;
            r_holder = right;
            pivot = InternalWindows[left].Time;
            BaseUI leftWindow, rightWindow;

            while (left < right)
            {
                while ((InternalWindows[right].Time >= pivot) && (left < right))
                {
                    right--;
                }

                if (left != right)
                {
                    leftWindow = InternalWindows[left];
                    InternalWindows[left] = InternalWindows[right];
                    InternalWindows[right] = leftWindow;
                    left++;
                }

                while ((InternalWindows[left].Time <= pivot) && (left < right))
                {
                    left++;
                }

                if (left != right)
                {
                    rightWindow = InternalWindows[right];
                    InternalWindows[right] = InternalWindows[left];
                    InternalWindows[left] = rightWindow;
                    right--;
                }
            }

            InternalWindows[left].Time = pivot;
            pivot = left;
            left = l_holder;
            right = r_holder;

            if (left < pivot)
            {
                Sort(left, pivot - 1);
            }

            if (right > pivot)
            {
                Sort(pivot + 1, right);
            }
        }

        #endregion

        /// <summary>
        /// Read Only Get function that returns the internal window list currently in use.
        /// </summary>
        public IList<BaseUI> GetWindows
        {
            get { return InternalWindows; }
        }

        #endregion

        #region Space HUD

        public StandardForm CreateHUDButtons()
        {
            var sf = new StandardForm(InternalWindows, _spriteBatch, _content, "");
            sf.SetBackgroundMaterial(@"GUI\Windows\ButtonBackground");
            sf.Size = new WindowSize(sf.BackgroundMaterial.Width, sf.BackgroundMaterial.Height);
            sf.SetFont(_textureManager.HUD_Font);
            sf.SetTitleColor(Color.Transparent);
            sf.IsTextVisible = false;
            sf.IsStatic = true;
            sf.Location = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width/2, 0);
            sf.OnUpdateEvent += HUDButtonsWindow_OnUpdateEvent;


            //System Button
            Button systemButton = CreateButton("", sf);
            systemButton.SetBackgroundMaterial(@"GUI\Windows\Button_SystemIcon");
            systemButton.OnClickEvent += systemButton_OnClickEvent;
            systemButton.OnUpdateEvent += systemButton_OnUpdateEvent;
            systemButton.Size = new WindowSize(systemButton.BackgroundMaterial.Width,
                                               systemButton.BackgroundMaterial.Height);
            systemButton.Location = new Vector2(sf.Location.X, sf.Location.Y + 4);
            systemButton.IsStatic = true;

            //Galaxy Button
            Button galaxyButton = CreateButton("", sf);
            galaxyButton.SetBackgroundMaterial(@"GUI\Windows\Button_GalaxyIcon");
            galaxyButton.OnClickEvent += galaxyButton_OnClickEvent;
            galaxyButton.OnUpdateEvent += galaxyButton_OnUpdateEvent;
            galaxyButton.Size = new WindowSize(galaxyButton.BackgroundMaterial.Width,
                                               galaxyButton.BackgroundMaterial.Height);
            galaxyButton.Location = new Vector2((sf.Location.X) + 1*galaxyButton.BackgroundMaterial.Width,
                                                sf.Location.Y + 4);
            galaxyButton.IsStatic = true;

            //Galaxy Button
            Button debugButton = CreateButton("", sf);
            debugButton.SetBackgroundMaterial(@"GUI\Windows\Button_GalaxyIcon");
            debugButton.OnClickEvent += debugButton_OnClickEvent;
            debugButton.OnUpdateEvent += debugButton_OnUpdateEvent;
            debugButton.Size = new WindowSize(debugButton.BackgroundMaterial.Width,
                                              debugButton.BackgroundMaterial.Height);
            debugButton.Location = new Vector2((sf.Location.X) + 1*debugButton.BackgroundMaterial.Width,
                                               sf.Location.Y + 4);
            debugButton.IsStatic = true;
            sf.Show();
            return sf;
        }
        public StandardForm CreateShipButtons()
        {
            int buttonHeight = 30;
            int buttonWidth = 30;
            int buttonSpacing = 4;
            int numAdded = 0;



            //Form to which buttons are added
            var sf = new StandardForm(InternalWindows, _spriteBatch, _content, "");
            //sf.SetBackgroundMaterial(TextureManager.testPoint);
            sf.SetBackgroundColor(Color.Transparent);
            sf.Size = new WindowSize(sf.BackgroundMaterial.Height, sf.BackgroundMaterial.Width);
            sf.SetFont(_textureManager.HUD_Font);
            sf.SetTitleColor(Color.Transparent);
            sf.IsTextVisible = false;
            sf.IsStatic = true;
            sf.Location = new Vector2(0, _spriteBatch.GraphicsDevice.Viewport.Height / 2);
            sf.OnUpdateEvent += ShipButtonsWindow_OnUpdateEvent;
            Button cargoButton;
            
            //System Button
            foreach (KeyValuePair<CargoTypes, int> kvp in _shipManager.PlayerShip.CargoAmounts)
            {
                
                cargoButton = CreateButton("", sf);

                switch (kvp.Key)
                {
                    case (CargoTypes.Biodome):
                        cargoButton.BackgroundMaterial = _textureManager.commandCenter;
                        break;
                    case (CargoTypes.AmbassadorMissile):
                        cargoButton.BackgroundMaterial = _textureManager.Pirani;
                        break;
                    case(CargoTypes.LaserTurret):
                        cargoButton.BackgroundMaterial = _textureManager.TurretHead;
                        break;
                    default:
                        cargoButton.SetBackgroundMaterial(@"GUI\Windows\Button_SystemIcon");
                        break;




                }

                
                
                
                cargoButton.OnClickEvent += cargoButton_OnClickEvent;
                cargoButton.OnUpdateEvent += cargoButton_OnUpdateEvent;
                cargoButton.Size = new WindowSize(buttonHeight,
                                                   buttonWidth);
                cargoButton.Location = new Vector2(sf.Location.X, sf.Location.Y + (buttonHeight + buttonSpacing) * numAdded + buttonSpacing);
                //cargoButton.Location = new Vector2(sf.Location.X, sf.Location.Y + numAdded * 100);
                cargoButton.data = kvp.Key;
                cargoButton.IsStatic = true;
                cargoButton.Text = kvp.Value.ToString();
                cargoButton.IsTextVisible = true;


                numAdded++;
            }


            sf.Size = new WindowSize(buttonWidth, (buttonHeight + buttonSpacing) * numAdded);


            sf.Show();
            return sf;
        }

        public StandardForm CreateSystemWindow()
        {
            var sf = new StandardForm(InternalWindows, _spriteBatch, _content, "System");
            sf.SetFont(_textureManager.HUD_Font);
            sf.SetTitleColor(Color.Transparent);
            sf.Text = "Options";
            sf.SetBackgroundMaterial(_textureManager.Window700x560);
            sf.Size = new WindowSize(sf.BackgroundMaterial.Width, sf.BackgroundMaterial.Height);
            sf.IsStatic = false;
            sf.Centered = true;
            sf.Transparency = 5;
            sf.OnUpdateEvent += SystemWindow_OnUpdateEvent;
            sf.Location = new Vector2(200, 50);
            sf.Show();
            return sf;
        }

        public StandardForm CreateNotification(string context)
        {
            var sf = new StandardForm(InternalWindows, _spriteBatch, _content, "Notification");
            sf.SetFont(_textureManager.HUD_Font);
            sf.SetTitleColor(Color.Transparent);
            sf.Text = context;
            sf.IsStatic = true;
            sf.Centered = true;
            sf.SetBackgroundMaterial(@"GUI\Windows\Notification250x100");
            sf.Size = new WindowSize(sf.BackgroundMaterial.Width, sf.BackgroundMaterial.Height);
            sf.Location = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width/2 - sf.Size.Width/2, 10);
            sf.WindowDown = false;
            sf.Timer = 1;
            sf.Transparency = 255;
            sf.Show();
            sf.OnUpdateEvent += NotificationUpdateEvent;
            return sf;
        }

        public StandardForm CreateHealthShieldBars()
        {
            var sf = new StandardForm(InternalWindows, _spriteBatch, _content, "");
            sf.SetFont(_textureManager.HUD_Font);
            sf.SetTitleColor(Color.Transparent);
            sf.IsTextVisible = false;
            sf.SetBackgroundMaterial(@"GUI\Windows\3Bars");
            sf.Size = new WindowSize(sf.BackgroundMaterial.Width, sf.BackgroundMaterial.Height);
            sf.IsStatic = false;
            sf.Centered = true;
            sf.Location = new Vector2(0, 0);

            //Static Button

            Button staticButton = CreateStaticButton("staticButton", sf);
            staticButton.Location = sf.Location;
            staticButton.Trigger = true;
            staticButton.Size = new WindowSize(staticButton.BackgroundMaterial.Width,
                                               staticButton.BackgroundMaterial.Height);

            //Health Bar
            Bar b2 = CreateBar("", sf);
            b2.SetBackgroundMaterial(@"GUI\Windows\Bar_Health");
            b2.OnUpdateEvent += HealthBarUpdate;
            b2.Size = new WindowSize(b2.BackgroundMaterial.Width, b2.BackgroundMaterial.Height);
            b2.Location = new Vector2(0, 0);
            b2.IsStatic = true;

            Label lbl = CreateLabel(ref sf, "Health");
            lbl.SetFont(_textureManager.HUD_Font);
            lbl.SetTextColor(Color.AntiqueWhite);
            lbl.OnUpdateEvent += HealthLabelUpdate;
            lbl.IsStatic = true;
            lbl.Text = "" + _shipManager.PlayerShip.CurrentHealth;
            //lbl.TextColor = Color.AntiqueWhite;
            lbl.Location =
                new Vector2(
                    (int) b2.Location.X + (b2.BackgroundMaterial.Width/2f) -
                    (_debugTextManager.GetFont().MeasureString(b2.Text).X/2f),
                    b2.Location.Y + (b2.BackgroundMaterial.Height/4f));


            //Shields Bar
            Bar b = CreateBar("", sf);
            b.SetBackgroundMaterial(@"GUI\Windows\Bar_Shields");
            b.OnUpdateEvent += ShieldsBarUpdate;
            b.Size = new WindowSize(b.BackgroundMaterial.Width, b.BackgroundMaterial.Height);
            b.IsStatic = true;
            b.Location = new Vector2(0, 33);

            Label lbl2 = CreateLabel(ref sf, "Shields");
            lbl2.SetFont(_textureManager.HUD_Font);
            lbl2.SetTextColor(Color.AntiqueWhite);
            lbl2.OnUpdateEvent += ShieldsLabelUpdate;
            lbl2.IsStatic = true;
            lbl2.Text = "" + _shipManager.PlayerShip.CurrentShields;
            //lbl2.TextColor = Color.LightBlue;
            lbl2.Location =
                new Vector2(
                    (int) b.Location.X + (b.BackgroundMaterial.Width/2f) -
                    (_debugTextManager.GetFont().MeasureString(b.Text).X / 2f),
                    b.Location.Y + (b.BackgroundMaterial.Height/4f));


            //Energy Bar
            Bar b3 = CreateBar("", sf);
            b3.SetBackgroundMaterial(@"GUI\Windows\Bar_Energy");
            b3.OnUpdateEvent += EnergyBarUpdate;
            b3.Size = new WindowSize(b3.BackgroundMaterial.Width, b3.BackgroundMaterial.Height);
            b3.IsStatic = true;
            b3.Location = new Vector2(0, 66);

            Label lbl3 = CreateLabel(ref sf, "Energy");
            lbl3.SetFont(_textureManager.HUD_Font);
            lbl3.SetTextColor(Color.AntiqueWhite);
            lbl3.OnUpdateEvent += EnergyLabelUpdate;
            lbl3.IsStatic = true;
            lbl3.Text = "" + _shipManager.PlayerShip.CurrentShields;
            //lbl3.TextColor = Color.LightBlue;
            lbl3.Location =
                new Vector2(
                    (int) b3.Location.X + (b3.BackgroundMaterial.Width/2f) -
                    (_debugTextManager.GetFont().MeasureString(b3.Text).X / 2f),
                    b3.Location.Y + (b3.BackgroundMaterial.Height/4f));


            sf.Show();

            return sf;
        }

        private void galaxyButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonHeld)
                return;
            var b = (Button) sender;


            if (b.Timer <= 0)
            {
                b.Timer = 10;
                b.SetBackgroundMaterial(_textureManager.Button_GalaxyIcon_Selected);
                for (int i = 0; i < GetWindows.Count; i++)
                {
                    if (GetWindows[i].Name == "Galaxy")
                    {
                        return;
                    }
                }
                InternalWindows.Add(CreateGalaxyWindow());
            }
        }

        private void galaxyButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Button) sender;
            b.Timer -= 1;
            if (b.Timer == 0)
            {
                b.SetBackgroundMaterial(_textureManager.Button_GalaxyIcon);
            }
            b.Location = new Vector2((b.Owner.Location.X) + 1*b.BackgroundMaterial.Width, b.Owner.Location.Y + 4);
        }

        private void systemButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonHeld)
                return;
            var b = (Button) sender;
            if (b.Timer <= 0)
            {
                b.Timer = 10;
                b.SetBackgroundMaterial(_textureManager.Button_SystemIcon_Selected);
                for (int i = 0; i < GetWindows.Count; i++)
                {
                    if (GetWindows[i].Name == "System")
                    {
                        return;
                    }
                }
                InternalWindows.Add(CreateSystemWindow());
            }
        }

        private void systemButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Button) sender;
            b.Timer -= 1;
            if (b.Timer == 0)
            {
                b.SetBackgroundMaterial(_textureManager.Button_SystemIcon);
            }
            b.Location = new Vector2((b.Owner.Location.X), b.Owner.Location.Y + 4);
        }

        private void cargoButton_OnClickEvent(BaseUI sender)
        {

            if (MouseManager.LeftButtonHeld)
                return;
            var b = (Button)sender;

            if (b.Timer <= 0)
            {
                b.Timer = 10;

                switch ((CargoTypes)b.data)
                {
                    case CargoTypes.Biodome:
                        //PlanetStateManager.colonizeMode = !PlanetStateManager.colonizeMode;
                        _bus.Publish(new MToggleColonizeMode());
                        break;

                    case CargoTypes.LaserTurret:
                        //if (GameStateManager.getState() == GameStates.planet)
                            //PlanetStateManager.turretPlacementMode = !PlanetStateManager.turretPlacementMode;
                        _bus.Publish(new MToggleStructurePlacementMode());
                        break;


                    default:
                        break;


                }

            }

        }

        private void cargoButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Button)sender;
            b.Timer -= 1;
            if (b.Timer == 0)
            {
                b.SetBackgroundColor(Color.White);
                
            }
        }

        private void debugButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonHeld)
                return;
            var b = (Button) sender;


            if (b.Timer <= 0)
            {
                b.Timer = 10;
                b.SetBackgroundMaterial(_textureManager.Button_GalaxyIcon_Selected);
                for (int i = 0; i < GetWindows.Count; i++)
                {
                    if (GetWindows[i].Name == "Galaxy")
                    {
                        return;
                    }
                }
                InternalWindows.Add(CreateGalaxyWindow());
            }
        }

        private void debugButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Button) sender;

            if (b.Timer == 0)
            {
                b.SetBackgroundMaterial(_textureManager.Button_GalaxyIcon);
                b.Timer -= 1;
            }
            else if (b.Timer > 0)
                b.Timer -= 1;
            b.Location = new Vector2((b.Owner.Location.X) + 1*b.BackgroundMaterial.Width, b.Owner.Location.Y + 4);
        }


        private void GalaxyWindow_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var sf = (StandardForm) sender;
            Button star;
            if (MouseManager.CurrentPosition.X > sf.Location.X && MouseManager.CurrentPosition.Y > sf.Location.Y
                && MouseManager.CurrentPosition.X < sf.Location.X + sf.BackgroundMaterial.Width &&
                MouseManager.CurrentPosition.Y < sf.Location.Y + 22
                && MouseManager.RightButtonDown)
            {
                RemoveWindowFromList(sf);
            }

            for (int i = 0; i < sf.ControlCollection.Count; i++)
            {
                star = (Button) sf.ControlCollection[i];
                if (star.Location.X > sf.Location.X + 36 && star.Location.Y > sf.Location.Y + 38
                    && star.Location.X < sf.Location.X + sf.BackgroundMaterial.Width - 36
                    && star.Location.Y < sf.Location.Y + sf.BackgroundMaterial.Height - 115)
                {
                    star.IsHidden = false;
                }
                else
                    star.IsHidden = true;
            }
        }

        private void GalaxyWindow_OnDrawEvent(SpriteBatch spriteBatch, BaseUI sender, GameTime gameTime)
        {
            var sf = (StandardForm) sender;
            Button star;
            GenerationStar child;
            Vector2 DrawPosition;
            float angle, length;
            for (int i = 0; i < sf.ControlCollection.Count; i++)
            {
                star = (Button) sf.ControlCollection[i];
                if (star.Location.X > sf.Location.X + 36 && star.Location.Y > sf.Location.Y + 38
                    && star.Location.X < sf.Location.X + sf.BackgroundMaterial.Width - 36
                    && star.Location.Y < sf.Location.Y + sf.BackgroundMaterial.Height - 115)
                {
                    for (int c = 0; c < star.Children.Count; c++)
                    {
                        child = star.Children[c];
                        //child.Pos += new Vector2(sf.Location.X + sf.BackgroundMaterial.Width / 2f, sf.Location.Y + sf.BackgroundMaterial.Height / 2f);
                        DrawPosition = child.Pos +
                                       new Vector2(sf.Location.X + sf.BackgroundMaterial.Width / 2f,
                                                   sf.Location.Y + sf.BackgroundMaterial.Height / 2f);
                        angle = (float)Math.Atan2(DrawPosition.Y - star.Location.Y, DrawPosition.X - star.Location.X);
                        length = Vector2.Distance(star.Location, DrawPosition);
                        spriteBatch.Begin();

                        spriteBatch.Draw(blank,
                                         star.Location +
                                         new Vector2(star.BackgroundMaterial.Width / 2f, star.BackgroundMaterial.Height / 2f),
                                         null, Color.LightGray,
                                         angle, Vector2.Zero, new Vector2(length, 2),
                                         SpriteEffects.None, 0);

                        spriteBatch.End();
                    }
                }
            }
        }

        private void SystemWindow_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            tempsf = (StandardForm) sender;
            if (MouseManager.CurrentPosition.X > tempsf.Location.X && MouseManager.CurrentPosition.Y > tempsf.Location.Y
                    && MouseManager.CurrentPosition.X < tempsf.Location.X + tempsf.BackgroundMaterial.Width &&
                MouseManager.CurrentPosition.Y < tempsf.Location.Y + 22
                    && MouseManager.RightButtonDown)
            {
                RemoveWindowFromList(tempsf);
            }
        }

        private void HUDButtonsWindow_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var sf = (StandardForm) sender;
            sf.Location = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width/2 - sf.BackgroundMaterial.Width/2f, 0);
        }
        private void ShipButtonsWindow_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var sf = (StandardForm)sender;
            sf.Location = new Vector2(0, _spriteBatch.GraphicsDevice.Viewport.Height / 2);
        }
        private void HealthLabelUpdate(BaseUI sender, GameTime gameTime)
        {
            var lbl = (Label) sender;
            lbl.Text = "" + _shipManager.PlayerShip.CurrentHealth;
        }

        private void ShieldsLabelUpdate(BaseUI sender, GameTime gameTime)
        {
            var lbl = (Label) sender;
            lbl.Text = "" + _shipManager.PlayerShip.CurrentShields;
        }

        private void ShieldsBarUpdate(BaseUI sender, GameTime gametime)
        {
            var b = (Bar) sender;
            float deci = (_shipManager.PlayerShip.CurrentShields /
                          (float)(_shipManager.PlayerShip.ShipStats.Shields + _shipManager.PlayerShip.maxShieldsBonus));
            b.Size = new WindowSize((int) (b.BackgroundMaterial.Width*deci), b.Size.Height);
        }

        private void EnergyLabelUpdate(BaseUI sender, GameTime gameTime)
        {
            var lbl = (Label) sender;
            lbl.Text = String.Format("{0:0.0}", _shipManager.PlayerShip.GetCurrentEnergy() / 10f) + "%";
        }

        private void EnergyBarUpdate(BaseUI sender, GameTime gametime)
        {
            var b = (Bar) sender;
            float deci = (_shipManager.PlayerShip.GetCurrentEnergy() / 1000f); // Change this to MaxEnergy
            b.Size = new WindowSize((int) (b.BackgroundMaterial.Width*deci), b.Size.Height);
        }

        private void HealthBarUpdate(BaseUI sender, GameTime gametime)
        {
            var b = (Bar) sender;
            float deci = (_shipManager.PlayerShip.CurrentHealth /
                          (float)(_shipManager.PlayerShip.ShipStats.Hull + _shipManager.PlayerShip.maxShieldsBonus));
            b.Size = new WindowSize((int) (b.BackgroundMaterial.Width*deci), b.Size.Height);
        }

        private void NotificationUpdateEvent(BaseUI sender, GameTime gametime)
        {
            var sf = (StandardForm) sender;
            sf.Location = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width/2 - sf.Size.Width/2, sf.Location.Y);
            if (sf.WindowDown == false)
            {
                sf.Transparency -= 4;

                if (sf.Transparency < 0)
                    sf.Transparency = 0;

                sf.Location = new Vector2(sf.Location.X, sf.Location.Y + (35 - sf.Location.Y)*.02f);

                if (sf.Location.Y > 30)
                {
                    sf.WindowDown = true;
                    sf.Timer = 300;
                }
            }
            if (sf.WindowDown)
                sf.Timer -= 1;

            if (sf.Timer < 0)
            {
                sf.Location = new Vector2(sf.Location.X, sf.Location.Y + (1 - sf.Location.Y)*.02f);
                sf.Transparency += 2;
                if (sf.Transparency > 255)
                    RemoveWindowFromList(sf);
            }

            sf.SetTextColor(new Color(255, 255, 255, sf.Transparency));
        }

        #endregion

        #region Port HUD

        public StandardForm CreatePortDropDownWindow(string window, int yPos, int numberOfButtons)
            //, List<PortInterfaceShip> ships)
        {
            for (int w = 0; w < GetWindows.Count; w++)
            {
                if (GetWindows[w].Name == "DropDownWindow")
                {
                    RemoveWindowFromList(GetWindows[w]);
                    w--;
                }
                else if (GetWindows[w].Name == "Port Interface")
                {
                    RemoveWindowFromList(GetWindows[w]);
                    w--;
                }
            }

            var DropDownWindow = new StandardForm(InternalWindows, _spriteBatch, _content, "DropDownWindow");
            DropDownWindow.SetBackgroundMaterial(@"GUI\Windows\340x700");
            DropDownWindow.Transparency = 100;
            DropDownWindow.HasTitleBar = false;
            DropDownWindow.Size = new WindowSize(DropDownWindow.BackgroundMaterial.Width,
                                                 DropDownWindow.BackgroundMaterial.Height);
            DropDownWindow.Location = Vector2.Zero;
            DropDownWindow.LerpLocation = new Vector2(0, yPos);
            DropDownWindow.OnUpdateEvent += PortDropDownWindow_OnUpdateEvent;
            DropDownWindow.IsStatic = true;
            DropDownWindow.OnUpdateEvent += DropDownWindow_OnUpdateEvent;

            // Make this an Enumerable type
            switch (window)
            {
                case "Shipyards":
                    for (int i = 0; i < _portStateManager.ShipsForSale.Count; i++) //ships.Count; i++)
                    {
                        Button ShipyardButton = CreateDataButton(_portStateManager.ShipsForSale[i].name,
                                                                 _portStateManager.ShipsForSale[i], DropDownWindow);
                            //ships[i].name, DropDownWindow);

                        ShipyardButton.SetBackgroundMaterial(_textureManager.Button_PortDropDownButton);
                        ShipyardButton.IsTextVisible = false;
                        ShipyardButton.Location = Vector2.Zero;
                        ShipyardButton.Transparency = 100;
                        ShipyardButton.LerpLocation = new Vector2(0, yPos + (yPos*i));
                        ShipyardButton.OnUpdateEvent += SelectedShipyardButton_OnUpdateEvent;
                        ShipyardButton.OnClickEvent += SelectedShipyardButton_OnClickEvent;
                        ShipyardButton.Size = new WindowSize(ShipyardButton.BackgroundMaterial.Width,
                                                             ShipyardButton.BackgroundMaterial.Height);
                        ShipyardButton.IsStatic = true;

                        Label ShipyardButton_lbl = CreateLabel(ref DropDownWindow, "ShipyardButton_lbl");
                        ShipyardButton_lbl.SetFont(_textureManager.HUD_Font);
                        ShipyardButton_lbl.SetTextColor(Color.Transparent);
                        ShipyardButton_lbl.DrawOrder = 0;
                        ShipyardButton_lbl.IsStatic = true;
                        ShipyardButton_lbl.Text = _portStateManager.ShipsForSale[i].name + "   Price: " +
                                                  _portStateManager.ShipsForSale[i].currentPrice.ToString();
                        ShipyardButton_lbl.Transparency = 255;
                        ShipyardButton_lbl.Location = new Vector2((ShipyardButton.BackgroundMaterial.Width/6f),
                                                                  ShipyardButton.LerpLocation.Y - 50);
                        ShipyardButton_lbl.OnUpdateEvent += SelectedShipyardButton_lbl_OnUpdateEvent;
                        ShipyardButton_lbl.LerpLocation = new Vector2(ShipyardButton_lbl.Location.X,
                                                                      ShipyardButton_lbl.Location.Y + 75);
                    }
                    break;
                case "Goods":
                    for (int i = 0; i < _portStateManager.GoodsForSale.Count; i++)
                    {
                        Button goodsButton = CreateButton("DropDownWindow", DropDownWindow);
                        goodsButton.SetBackgroundMaterial(_textureManager.Button_PortDropDownButton);
                        goodsButton.OnClickEvent += selectedGoodsButton_OnClickEvent;
                        goodsButton.IsTextVisible = false;
                        goodsButton.Location = Vector2.Zero;
                        goodsButton.Transparency = 100;
                        goodsButton.LerpLocation = new Vector2(0, yPos + (yPos*i));
                        goodsButton.OnUpdateEvent += SelectedOutfitterButton_OnUpdateEvent;
                        goodsButton.Size = new WindowSize(goodsButton.BackgroundMaterial.Width,
                                                          goodsButton.BackgroundMaterial.Height);
                        goodsButton.IsStatic = true;

                        Label goodsButton_lbl = CreateLabel(ref DropDownWindow, "goodsButton_lbl");
                        goodsButton_lbl.SetFont(_textureManager.HUD_Font);
                        goodsButton_lbl.SetTextColor(Color.Transparent);
                        goodsButton_lbl.DrawOrder = 0;
                        goodsButton_lbl.IsStatic = true;
                        goodsButton_lbl.Text = _portStateManager.ShipsForSale[i].name + "   Price: " +
                                               _portStateManager.ShipsForSale[i].currentPrice.ToString();
                        goodsButton_lbl.Transparency = 255;
                        goodsButton_lbl.Location = new Vector2((goodsButton.BackgroundMaterial.Width/6f),
                                                               goodsButton.LerpLocation.Y - 50);
                        goodsButton_lbl.OnUpdateEvent += selectedGoodsButton_lbl_OnUpdateEvent;
                        goodsButton_lbl.LerpLocation = new Vector2(goodsButton.Location.X, goodsButton.Location.Y + 75);
                    }
                    break;
                case "Outfitter":
                    break;
                case "Alpha":
                    break;
            }

            DropDownWindow.Show();
            return DropDownWindow;
        }

        private void DropDownWindow_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
        }

        private void SelectedShipyardButton_lbl_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Label) sender;
            if (Math.Abs(b.Location.Y - b.LerpLocation.Y) > 1)
            {
                b.Location = new Vector2(b.Location.X - ((b.Location.X - b.LerpLocation.X) * .08f),
                                         b.Location.Y - ((b.Location.Y - b.LerpLocation.Y) * .08f));
            }
            b.Transparency -= 5;
            b.SetTextColor(new Color(255, 255, 255) * -(b.Transparency / 255));
        }

        private void selectedGoodsButton_lbl_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Label) sender;
            if (Math.Abs(b.Location.Y - b.LerpLocation.Y) > 1)
            {
                b.Location = new Vector2(b.Location.X - ((b.Location.X - b.LerpLocation.X) * .08f),
                                         b.Location.Y - ((b.Location.Y - b.LerpLocation.Y) * .08f));
            }
            b.Transparency -= 5;
            b.SetTextColor(new Color(255, 255, 255) * -(b.Transparency / 255));
        }

        private void SelectedOutfitterButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Button) sender;
            if (Math.Abs(b.Location.Y - b.LerpLocation.Y) > 1)
            {
                b.Location = new Vector2(b.Location.X - ((b.Location.X - b.LerpLocation.X) * .08f),
                                         b.Location.Y - ((b.Location.Y - b.LerpLocation.Y) * .08f));
            }
            if (b.Transparency > 20)
                b.Transparency -= 2;
        }

        private void SelectedShipyardButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var b = (Button) sender;
            if (Math.Abs(b.Location.Y - b.LerpLocation.Y) > 1)
            {
                b.Location = new Vector2(b.Location.X - ((b.Location.X - b.LerpLocation.X) * .08f),
                                         b.Location.Y - ((b.Location.Y - b.LerpLocation.Y) * .08f));
            }
            if (b.Transparency > 20)
                b.Transparency -= 1;
        }

        private void PortDropDownWindow_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var sf = (StandardForm) sender;
            if (Math.Abs(sf.Location.Y - sf.LerpLocation.Y) > 1)
            {
                sf.Location = new Vector2(sf.Location.X - ((sf.Location.X - sf.LerpLocation.X) * .02f),
                                          sf.Location.Y - ((sf.Location.Y - sf.LerpLocation.Y) * .02f));
            }
            if (sf.Transparency > 20)
                sf.Transparency -= 1;
        }

        private void ShipyardButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonPressed)
            {
                var b = (Button) sender;
                InternalWindows.Add(CreatePortDropDownWindow("Shipyards", b.BackgroundMaterial.Height, 5));
            }
        }

        private void SelectedShipyardButton_OnClickEvent(BaseUI sender)
        {
            var b = (Button) sender;
            var g = (ShipGood) b.data;
            if (MouseManager.LeftButtonPressed)
            {
                InternalWindows.Add(CreatePortWindow(g.name, (byte) g.shipType, (int) g.currentPrice,
                                                     g.shields, g.hull, g.cargo, g.energy, g,
                                                     g.description));
            }
        }

        private void selectedGoodsButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonPressed)
            {
                InternalWindows.Add(CreatePortWindow("test", 0, 1000, 1000, 1000, 1000, 1000, new Good(0, "null", 0, 0)));
            }
        }


        private void GoodsButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonPressed)
            {
                var b = (Button) sender;
                InternalWindows.Add(CreatePortDropDownWindow("Goods", b.BackgroundMaterial.Height, 3));
            }
        }

        private void ExitButton_OnClickEvent(BaseUI sender)
        {
            if (MouseManager.LeftButtonPressed)
            {
                if (!_shipManager.PlayerPilot.IsDead)
                {
                    NetOutgoingMessage msg = _clientManager.Client.CreateMessage();
                    msg.Write((byte) MessageTypes.UndockRequest);
                    msg.Write(_shipManager.PlayerShip.shipID);
                    _clientManager.Client.SendMessage(msg, _clientManager.CurrentSlaveConnection, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }

        public StandardForm CreatePortWindow(string shipName, byte sprite, int price, int shields, int hull,
                                                    int cargoholds, int energy, Good good,
                                                    string description = "A basic ship")
        {
            var sf = new StandardForm(InternalWindows, _spriteBatch, _content, "Port Interface");
            sf.SetFont(_textureManager.HUD_Font);
            sf.SetTitleColor(Color.Transparent);
            sf.Centered = false;
            sf.Text = shipName;
            sf.SetBackgroundMaterial(@"GUI\Windows\Port_ShipWindow");
            sf.Size = new WindowSize(sf.BackgroundMaterial.Width, sf.BackgroundMaterial.Height);
            sf.IsStatic = false;
            sf.Transparency = 5;
            //sf.OnUpdateEvent += new BaseForm.OnUpdateHandle(GalaxyWindow_OnUpdateEvent);
            sf.Location = new Vector2(220, 60);

            Label priceLbl = CreateLabel(ref sf, "PriceLabel");
            priceLbl.SetFont(_textureManager.HUD_Font);
            priceLbl.SetTextColor(Color.White);
            priceLbl.DrawOrder = 0;
            priceLbl.IsStatic = true;
            priceLbl.Text = "Price $" + price;
            priceLbl.Transparency = 255;
            priceLbl.Location = sf.Location +
                                new Vector2(sf.Size.Width - _textureManager.HUD_Font.MeasureString(priceLbl.Text).X - 3,
                                            2);
            //priceLbl.OnUpdateEvent += new BaseControl.OnUpdateHandler(SelectedShipyardButton_lbl_OnUpdateEvent);

            int lineLength = 200; // horizontal maximum of each line before split

            // grabs the description and breaks it up into multiple lines
            if (description.Length == 0 || string.IsNullOrWhiteSpace(description))
                Logger.Log(Log_Type.ERROR, "null ship description added for " + shipName); // avoid crashes
            else
            {
                string split = LineSplitParagraph(lineLength, description); // puts everything into the designated area :)

                Label descriptionLbl = CreateLabel(ref sf, "description");
                descriptionLbl.SetFont(_textureManager.descriptionFont);
                descriptionLbl.SetTextColor(Color.White);
                descriptionLbl.DrawOrder = 0;
                descriptionLbl.IsStatic = true;
                descriptionLbl.Text = split;
                descriptionLbl.Transparency = 255;
                descriptionLbl.Location = sf.Location + new Vector2(8, 26);
            }
            // Right Side Information, shields etc
            Label shieldsLbl = CreateLabel(ref sf, "shields");
            shieldsLbl.SetFont(_textureManager.HUD_Font);
            shieldsLbl.SetTextColor(Color.White);
            shieldsLbl.DrawOrder = 0;
            shieldsLbl.IsStatic = true;
            shieldsLbl.Text = "" + shields;
            shieldsLbl.Transparency = 255;
            shieldsLbl.Location = sf.Location + new Vector2(390 - shieldsLbl.Font.MeasureString(shieldsLbl.Text).X, 30);

            Label hullLbl = CreateLabel(ref sf, "hull");
            hullLbl.SetFont(_textureManager.HUD_Font);
            hullLbl.SetTextColor(Color.White);
            hullLbl.DrawOrder = 0;
            hullLbl.IsStatic = true;
            hullLbl.Text = "" + hull;
            hullLbl.Transparency = 255;
            hullLbl.Location = sf.Location + new Vector2(390 - hullLbl.Font.MeasureString(hullLbl.Text).X, 60);

            Label energyLbl = CreateLabel(ref sf, "energy");
            energyLbl.SetFont(_textureManager.HUD_Font);
            energyLbl.SetTextColor(Color.White);
            energyLbl.DrawOrder = 0;
            energyLbl.IsStatic = true;
            energyLbl.Text = "" + energy;
            energyLbl.Transparency = 255;
            energyLbl.Location = sf.Location + new Vector2(390 - energyLbl.Font.MeasureString(energyLbl.Text).X, 90);

            Label cargoLbl = CreateLabel(ref sf, "cargo");
            cargoLbl.SetFont(_textureManager.HUD_Font);
            cargoLbl.SetTextColor(Color.White);
            cargoLbl.DrawOrder = 0;
            cargoLbl.IsStatic = true;
            cargoLbl.Text = "" + cargoholds;
            cargoLbl.Transparency = 255;
            cargoLbl.Location = sf.Location + new Vector2(390 - cargoLbl.Font.MeasureString(cargoLbl.Text).X, 120);

            Label valueLbl = CreateLabel(ref sf, "value");
            valueLbl.SetFont(_textureManager.HUD_Font);
            valueLbl.SetTextColor(Color.White);
            valueLbl.DrawOrder = 0;
            valueLbl.IsStatic = true;
            valueLbl.Text = ""; // +value;
            valueLbl.Transparency = 255;
            valueLbl.Location = sf.Location + new Vector2(390 - valueLbl.Font.MeasureString(valueLbl.Text).X, 150);

            //Label priceLbl = new Label(spriteBatch, Content, price + "", sf);
            //priceLbl.Location = sf.Location + new Vector2(sf.Size.Width - priceLbl.Size.Width - 10, 0);
            Button b = CreateButton("RickClickCloseButton", sf);
            b.OnUpdateEvent += RightClickCloseButton_OnUpdateEvent;
            b.Size = new WindowSize(sf.BackgroundMaterial.Width, verticalOffset);
            b.Location = sf.Location;
            b.SetBackgroundColor(Color.Transparent);
            b.IsStatic = true;

            Button buyButton = CreateDataButton("PurchaseDataButton", good, sf); // Creates a button that holds a good, so that we can buy stuff
            buyButton.OnClickEvent += buyButton_OnClickEvent;
            buyButton.SetBackgroundMaterial(_textureManager.Button_Purchase);
            buyButton.Size = new WindowSize(buyButton.BackgroundMaterial.Width, buyButton.BackgroundMaterial.Height);
            buyButton.Location = new Vector2(sf.Location.X + 10,
                                             sf.Location.Y + sf.Size.Height - buyButton.Size.Height - 10);
            buyButton.IsStatic = true;

            sf.Show();
            return sf;
        }


        private void RightClickCloseButton_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            tempbutton = (Button) sender;
            if (MouseManager.CurrentPosition.X > tempbutton.Location.X &&
                MouseManager.CurrentPosition.Y > tempbutton.Location.Y
                && MouseManager.CurrentPosition.X < tempbutton.Location.X + tempbutton.Size.Width &&
                MouseManager.CurrentPosition.Y < tempbutton.Location.Y + tempbutton.Size.Height
                && MouseManager.RightButtonDown)
            {
                RemoveWindowFromList(tempbutton.Owner);
            }
        }

        public StandardForm CreateAuxiliaryInformationPanels()
        {
            StandardForm sf = CreateStandardForm("Auxiliary");

            sf.SetFont(_textureManager.HUD_Font);
            sf.HasTitleBar = false;
            sf.IsTextVisible = false;

            sf.SetBackgroundColor(Color.Transparent);
            sf.IsStatic = true;
            sf.Centered = true;
            sf.OnUpdateEvent += portDockedHud_OnUpdateEvent;
            sf.Location = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width - _textureManager.DockableTextBar.Width,
                                      _spriteBatch.GraphicsDevice.Viewport.Height - _textureManager.DockableTextBar.Height);
                // Fuck this shit, hardcoded for now.

            int bars = 0;
            // Money Bar //
            Bar b = CreateBar("Money Display", sf);
            b.SetBackgroundMaterial(_textureManager.DockableTextBar);
            b.Location = new Vector2(sf.Location.X, sf.Location.Y + (b.BackgroundMaterial.Height*bars));
            b.OnUpdateEvent += BarAlign;
            b.Size = new WindowSize(b.BackgroundMaterial.Width, b.BackgroundMaterial.Height);
            b.IsStatic = true;

            Label bl = AddLabelToTextPanel(b, sf); // This is where the actual text is stored

            // Here we set all of the variables
            bl.OnUpdateEvent += portCashDisplay_OnUpdateEvent;
            bl.Text = "Money: " + _shipManager.currentCash;
            bl.Location =
                new Vector2(
                    (int)b.Location.X + b.BackgroundMaterial.Width - RightHandMargin -
                    (_debugTextManager.GetFont().MeasureString(bl.Text).X), b.Location.Y);


            bars++;
            // Money Bar //
            Bar b2 = CreateBar("Shields Display", sf);
            b2.SetBackgroundMaterial(_textureManager.DockableTextBar);
            b2.Location = new Vector2(sf.Location.X,
                                      sf.Location.Y - (b2.BackgroundMaterial.Height*bars) + ((numberOfBars)*2) - 1);
            b2.OnUpdateEvent += BarAlign;
            b2.Size = new WindowSize(b2.BackgroundMaterial.Width, b2.BackgroundMaterial.Height);
            b2.IsStatic = true;

            Label bl2 = AddLabelToTextPanel(b2, sf); // This is where the actual text is stored

            // Here we set all of the variables
            bl2.OnUpdateEvent += CurrentShields_OnUpdateEvent;
            if(_shipManager.PlayerShip != null)
                bl2.Text = GetConcatinatedString("Hull: " + _shipManager.PlayerShip.CurrentHealth,
                                                 "Shields: " + _shipManager.PlayerShip.CurrentShields,
                                                 bl2.Font, bl2.Owner.Size.Width - RightHandMargin, RightHandMargin);
            bl2.Location =
                new Vector2(
                    (int)b2.Location.X + b2.BackgroundMaterial.Width - RightHandMargin -
                    (_debugTextManager.GetFont().MeasureString(bl2.Text).X), b2.Location.Y);


            // Keeps from going off screen on resize
            sf.Size = new WindowSize(_textureManager.DockableTextBar.Width, _textureManager.DockableTextBar.Height*bars);

            sf.Show();
            return sf;
        }

        /// <summary>
        /// Takes two strings and puts them into a given contraint space.
        /// </summary>
        /// <param name="s1">string at beginning</param>
        /// <param name="s2">string at end</param>
        /// <param name="font">font to measure with</param>
        /// <param name="pixels">constraint space</param>
        /// <param name="padding">padding on left</param>
        /// <returns></returns>
        private string GetConcatinatedString(string s1, string s2, SpriteFont font, int pixels, int padding)
        {
            string concat;
            var size =
                (int)
                Math.Floor((pixels - (int)font.MeasureString(s1).X - (int)font.MeasureString(s2).X - padding) /
                           font.MeasureString(" ").X);

            concat = s1;
            for (int i = 0; i < size; i++)
                concat += " ";
            concat += s2;
            return concat;
        }

        private void CurrentShields_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var lbl = (Label) sender;
            lbl.Text = GetConcatinatedString("Hull: " + _shipManager.PlayerShip.CurrentHealth,
                                             "Shields: " + _shipManager.PlayerShip.CurrentShields,
                                             lbl.Font, lbl.Owner.Size.Width - RightHandMargin, RightHandMargin);
            lbl.Location =
                new Vector2(
                    (int) lbl.Owner.Location.X + lbl.Owner.Size.Width - RightHandMargin -
                    (_debugTextManager.GetFont().MeasureString(lbl.Text).X), lbl.Location.Y);
        }

        private void portCashDisplay_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            var lbl = (Label) sender;
            lbl.Text =
                lbl.Text =
                GetConcatinatedString("Ship: " + _shipManager.PlayerShip.shipName, "Money: " + _shipManager.currentCash,
                                      lbl.Font, lbl.Owner.Size.Width - RightHandMargin, RightHandMargin);
            //"Money: " + ShipManager.currentCash;
            lbl.Location =
                new Vector2(
                    (int)lbl.Owner.Location.X + lbl.Owner.Size.Width - RightHandMargin -
                    (_debugTextManager.GetFont().MeasureString(lbl.Text).X), lbl.Location.Y);
        }

        private void portDockedHud_OnUpdateEvent(BaseUI sender, GameTime gameTime)
        {
            sender.Location = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width - sender.Size.Width,
                                          _spriteBatch.GraphicsDevice.Viewport.Height - sender.Size.Height);
        }

        private string LineSplitParagraph(int lineLength, string description)
        {
            if (description.Length == 0)
                return "";
            int currentLineLength = 0;
            string descriptionWithBreaks = ""; // end line to be written
            string concat = "";
            string[] descriptionParsed = description.Split(' ');
            for (int wordNumber = 0; wordNumber < descriptionParsed.Count(); wordNumber++)
            {
                var wordLength = (int)_textureManager.descriptionFont.MeasureString(descriptionParsed[wordNumber]).X;
                    // length of word
                if (currentLineLength + wordLength < lineLength) // check if longer than line allows for
                {
                    currentLineLength += wordLength;
                    concat += descriptionParsed[wordNumber] + " ";

                    if (wordNumber == descriptionParsed.Count() - 1) // Prints last line
                        descriptionWithBreaks += concat;
                    continue;
                }
                descriptionWithBreaks += concat + "\n";
                currentLineLength = 0;
                concat = "";
                wordNumber--;
            }

            return descriptionWithBreaks;
        }

        #endregion

        public void Handle(MSetWindowListMessage message)
        {
            SetWindowList(message.Windows);
        }
    }
}