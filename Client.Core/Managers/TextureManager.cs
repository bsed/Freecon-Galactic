using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Managers
{
    public class TextureManager
    {
        #region Ship Textures

        //3D
        Texture2D _superCoolAwesome3DShipTexture;
        public Model SuperCoolAwesome3DShipModel;


        public Texture2D shield_sfx;
        public Texture2D Battlecruiser;
        public Texture2D Penguin;
        public Texture2D ZYVariantBarge;
        public Texture2D Reaper;
        public Texture2D Mothership, MothershipOutline;

        // Release-worthy Ships
        public Texture2D JeythAssualt;
        public Texture2D JeythSupport;
        public Texture2D JeythFreighter;
        public Texture2D JeythTactical;
        public Texture2D NebulanCruiser;
        public Texture2D Pirani;
        public Texture2D SMFighter;
        public Texture2D Rasputin;
        public Texture2D Dread;

        // Planets
        public Texture2D Ice;
        public Texture2D Desert;
        public Texture2D DesertTwo;
        public Texture2D Rocky;
        public Texture2D Barren;
        public Texture2D Gray;
        public Texture2D Red;
        public Texture2D ColdGasGiant1;
        public Texture2D IceGiant;
        public Texture2D HotGasGiant;
        public Texture2D Radioactive;
        private Texture2D Unstable;
        public Texture2D OceanicLarge;
        public Texture2D Frozen;

        // Small
        public Texture2D Crystalline;
        public Texture2D Gaia;
        public Texture2D OceanicSmall;

        // Special
        public Texture2D Paradise;

        // Shadows
        public Texture2D Shadow_Barren;
        public Texture2D Shadow_ColdGasGiant;
        public Texture2D Shadow_Base300px;
        public Texture2D Shadow_OceanicSmall;
        public Texture2D Shadow_Gaia;
        public Texture2D Shadow_Crystalline;
        public Texture2D Shadow_water;

        // Suns
        public Texture2D YellowSun;
        public Texture2D BlueSun;


        // Ports
        public Texture2D Port;

        // Dead Ships
        public Texture2D deadTex; //Not implemented

        #endregion

        #region Space Textures
        public Texture2D Background { get; private set;  }
        public Texture2D Warphole;
        #endregion

        #region Planet Textures
        public Texture2D
            Wall,
            Ground,
            Cracked,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            LeftWall,
            RightWall,
            BottomWall,
            TopWall,
            TopBottom,
            LeftRight,
            LeftEnd,
            TopEnd,
            RightEnd,
            BottomEnd;


        #endregion

        #region Port Textures

        public Texture2D Port_StationHD;

        #endregion

        #region HUD Textures

        // UI Fonts
        public SpriteFont HUD_Font;
        public SpriteFont Text_Font;

        // Space UI Elements
        public Texture2D DockableTextPanel;
        public Texture2D DockableTextBar;
        public Texture2D staticWindowTexture_Off; // Used to dock elements
        public Texture2D staticWindowTexture_On; // Used to dock elements

        // Windows for OnClick Hud Buttons
        public Texture2D Window700x560;
        public Texture2D Window700x560Galaxy;

        // Icons for top bar
        public Texture2D Button_GalaxyIcon;
        public Texture2D Button_GalaxyIcon_Selected;
        public Texture2D Button_SystemIcon;
        public Texture2D Button_SystemIcon_Selected;
        public Texture2D Button_DebugIcon;

        // Port UI Elements
        public Texture2D Button_PortDropDownButton;
        public Texture2D Button_Purchase;

        #endregion

        #region Planet Layouts
        public Texture2D Layout1;
        public Texture2D layout2;
        public Texture2D layout3;
        public Texture2D testLayout;

        #endregion

        #region Structures
        public Texture2D TurretHead;
        public Texture2D CommandCenter;
        public Texture2D TurretBase;
        public Texture2D Biodome;
        #endregion

        #region Projectiles

        static public Texture2D Laser { get; private set; }
        static public Texture2D Orb { get; private set; }
        static public Texture2D PlasmaCannon { get; private set; }
        static public Texture2D NaniteLauncher { get; private set; }
        static public Texture2D Ambassador { get; private set; }
        static public Texture2D Hellhound { get; private set; }
        static public Texture2D MissileType1 { get; private set; }
        static public Texture2D MissileType2 { get; private set; }
        static public Texture2D MissileType3 { get; private set; }
        static public Texture2D MissileType4 { get; private set; }
        #endregion

        #region Cursors

        public static Texture2D TargetCursor;
        
        #endregion
                

        #region Other

        static public Texture2D redPoint;
        static public Texture2D greenPoint;
        public SpriteFont descriptionFont;
        public Texture2D tex_DotW;
        public Texture2D FloatingModule;
        public Texture2D MineOn;
        public Texture2D MineOff;
        public SpriteFont DefaultDrawFont;

        #endregion

        public TextureManager(ContentManager content)
        {
            #region Ships

            Battlecruiser = content.Load<Texture2D>(@"Client.Monogame.Content/Ships/BC");
            Reaper = content.Load<Texture2D>(@"Client.Monogame.Content/Ships/jeyth1_small");
            ZYVariantBarge = content.Load<Texture2D>(@"Client.Monogame.Content/Ships/zyv");
            Penguin = content.Load<Texture2D>(@"Client.Monogame.Content/Ships/jeyth.skinned.small");
            Dread = content.Load<Texture2D>(@"Client.Monogame.Content/Ships/dread_game");
            Mothership = content.Load<Texture2D>(@"Client.Monogame.Content/Space/mothership_side");
            MothershipOutline = content.Load<Texture2D>(@"Client.Monogame.Content/Space/mothership_side_box");
            redPoint = content.Load<Texture2D>(@"Client.Monogame.Content/Textures/RedPoint");
            greenPoint = content.Load<Texture2D>(@"Client.Monogame.Content/Textures/GreenPoint");

            #endregion

            #region HUD Texture/Asset Loading

            HUD_Font = content.Load<SpriteFont>(@"Client.Monogame.Content/GUI\HUD_Font");
            staticWindowTexture_Off = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_StaticWindow_Off");
            staticWindowTexture_On = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_StaticWindow_On");
            Window700x560 = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\700x560");
            Window700x560Galaxy = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\700x560Galaxy");
            Button_GalaxyIcon = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_GalaxyIcon");
            Button_GalaxyIcon_Selected = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_GalaxyIcon_Selected");
            Button_SystemIcon = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_SystemIcon");
            Button_SystemIcon_Selected = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_SystemIcon_Selected");
            Button_GalaxyIcon = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_DebugIcon");
            Button_PortDropDownButton = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_PortDropDownButton");
            Button_Purchase = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\Button_Purchase");

            // Dockable UI Element for underneath Radar
            DockableTextPanel = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\DockablePanel-Brushed");
            DockableTextBar = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\DockablePanel-Bar");

            #endregion

            shield_sfx = content.Load<Texture2D>(@"Client.Monogame.Content/Textures/shield_sfx");
            descriptionFont = content.Load<SpriteFont>(@"Client.Monogame.Content/GUI/ChatboxFont");

            #region Space Textures
            Background = content.Load<Texture2D>(@"Client.Monogame.Content/Space/background1");
            Warphole = content.Load<Texture2D>(@"Client.Monogame.Content/Space/WarpHole");

            Ice = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/ice2_300");
            Desert = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/desert_300");
            DesertTwo = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/desertTwo_300px");
            Rocky = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/rocky_300px");
            Barren = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Barren_300");
            Gray = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/gray_300px");
            Red = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/red_300px");
            ColdGasGiant1 = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/GasGiant1a_300px");
            IceGiant = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/icegiant_300");
            HotGasGiant = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/hotgasgiant_300px");
            Radioactive = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Terra_300px");
            Unstable = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/unstable_300px");
            OceanicLarge = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Water_300px");
            Frozen = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/frozen_300px");
            // Small
            Crystalline = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Crystalline-200px");
            Gaia = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Gaia-200px");
            OceanicSmall = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Oceanic-187px");
            // Special
            Paradise = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Paradise_300");

            // Shadows
            //Shadow_Barren = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/Radioactive_Shadow");
            //Shadow_ColdGasGiant = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/GasGiant_Shadow_300px");
            Shadow_Barren = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/water-shadow_300");
            Shadow_ColdGasGiant = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/water-shadow_300");
            Shadow_Base300px = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/water-shadow_300");
            Shadow_Crystalline = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/water-shadow_300");
            Shadow_Gaia = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/Gaia-Shadow");
            Shadow_OceanicSmall = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/OceanicShadow");
            //Shadow_Base300px = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/Radioactive_Shadow");
            Shadow_water = content.Load<Texture2D>(@"Client.Monogame.Content/Planet/Shadows/water-shadow_300");
            // Objects
            Port = content.Load<Texture2D>(@"Client.Monogame.Content/Space/port_360p");
            YellowSun = content.Load<Texture2D>(@"Client.Monogame.Content/Space/sun_yellow2_fullres");
            BlueSun = content.Load<Texture2D>(@"Client.Monogame.Content/Space/Sun_Blue_400");
            #endregion

            #region Port Textures

            Port_StationHD = content.Load<Texture2D>(@"Client.Monogame.Content/GUI\Windows\stationhd");

            #endregion

            #region Planet Textures

            Ground = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/Ground");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Ground");
            // Loads all of the textures.
            Wall = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/Wall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall");
            Cracked = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/Wall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Cracked_A");
            TopLeft = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/TopLeftWall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_TopLeft_Corner");
            TopRight = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/TopRightWall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_TopRight_Corner");
            BottomLeft = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/BottomLeftCorner");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_BottomLeft_Corner");
            BottomRight = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/BottomRightCorner");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_BottomRight_Corner");
            LeftWall = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/LeftWall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_LeftWall");
            RightWall = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/RightWall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_RightWall");
            BottomWall = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/BottomWall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_BottomWall");
            TopWall = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/TopWall");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_Wall_TopWall");
            TopBottom = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/TopBottom");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_TopBottom");
            LeftRight = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/LeftRight");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_LeftRight");
            LeftEnd = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/LeftEnd");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_leftEnd");
            TopEnd = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/TopEnd");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_topEnd");
            RightEnd = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/RightEnd");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_rightEnd");
            BottomEnd = content.Load<Texture2D>(@"Client.Monogame.Content/Tileset/Lava/BottomEnd");//@"Client.Monogame.Content/Tileset/Earth/Tile_Earth_bottomEnd");
            

            #endregion

            #region Planet Layouts
            Layout1 = content.Load<Texture2D>(@"Client.Monogame.Content/Modules/4wayC");
            layout2 = content.Load<Texture2D>(@"Client.Monogame.Content/Modules/parse");
            layout3 = content.Load<Texture2D>(@"Client.Monogame.Content/Modules/tyest1");
            #endregion

            #region Projectiles

            Laser = content.Load<Texture2D>(@"Client.Monogame.Content/Weapons/lasershit");
            PlasmaCannon = content.Load<Texture2D>(@"Client.Monogame.Content/Projectiles/plasma cannon");
            NaniteLauncher = content.Load<Texture2D>(@"Client.Monogame.Content/Buildings/Command Center");
            Ambassador = content.Load<Texture2D>(@"Client.Monogame.Content/Projectiles/Missile");
            Hellhound = content.Load<Texture2D>(@"Client.Monogame.Content/Projectiles/hellhound");
            MissileType1 = content.Load<Texture2D>(@"Client.Monogame.Content/Projectiles/MissileType1");
            MissileType2 = content.Load<Texture2D>(@"Client.Monogame.Content/Projectiles/MissileType2");
            MissileType3 = content.Load<Texture2D>(@"Client.Monogame.Content/Projectiles/MissileType3");
            MissileType4 = content.Load<Texture2D>(@"Client.Monogame.Content/Projectiles/MissileType4");
            #endregion

            #region Structures

            tex_DotW = content.Load<Texture2D>(@"Client.Monogame.Content/GUI/dot_w");;
            CommandCenter = content.Load<Texture2D>(@"Client.Monogame.Content/Buildings/command center");
            Biodome = content.Load<Texture2D>(@"Client.Monogame.Content/Buildings/Biodome");
            
            TurretBase = content.Load<Texture2D>(@"Client.Monogame.Content/Turrets/turret_2_base_128px");//@"Client.Monogame.Content/Planet/turret_grass_96_final");
            TurretHead = content.Load<Texture2D>(@"Client.Monogame.Content/Turrets/turret_2_head_128px");
            #endregion

            #region Cursors

            TargetCursor = content.Load<Texture2D>(@"Client.Monogame.Content/Cursors/reticle-original");

            #endregion

            #region Other
            FloatingModule = content.Load<Texture2D>(@"Client.Monogame.Content/GUI/dot_w");
            MineOn = content.Load<Texture2D>(@"Client.Monogame.Content/Space/mines-on");
            MineOff = content.Load<Texture2D>(@"Client.Monogame.Content/Space/mines-off");
            DefaultDrawFont = content.Load<SpriteFont>(@"Client.Monogame.Content/GUI/drawFont");
            #endregion

            testLayout = content.Load<Texture2D>(@"Client.Monogame.Content/somelevel");

            Load3DAssets(content);
        }

        void Load3DAssets(ContentManager Content)
        {
            //SuperCoolAwesome3DShipModel = Content.Load<Model>(@"Ships/Ship9");
            //_superCoolAwesome3DShipTexture = Content.Load<Texture2D>("Textures/wedge_p1_diff_v1");
            //SetTexture(SuperCoolAwesome3DShipModel, _superCoolAwesome3DShipTexture);
            
        }

        void SetTexture(Model model, Texture2D texture)
        {
            var be = model.Meshes[0].Effects[0] as BasicEffect;//We're probably going to need to specialize this soon.
            be.TextureEnabled = true;
            be.Texture = texture;
        }


    }

    /// <summary>
    /// Drawing order to prevent things like turret bases drawn on top of turret heads
    /// </summary>
    public class RenderDepths
    {
        //Let's try to keep these in order. Draw() method uses a simple > check to determine order
        //Values must be between 0 and 1, 0 is front, 1 is back
        public static readonly float Background = 1;
        public static readonly float TurretBase = .999f;
        public static readonly float TurretHead = .998f;


        public static readonly float NetworkShip = .003f;
        public static readonly float PlayerShip = .002f;
        public static readonly float Cursor = .001f;


    }
    

}
