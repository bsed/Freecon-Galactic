#define ADMIN

using System;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Mathematics;
using Freecon.Client.Mathematics.Effects;
using Autofac;
using Freecon.Client.Base;
using Freecon.Client.Core;
using System.Threading.Tasks;
using Freecon.Core.Configs;
using Freecon.Client.View.CefSharp;
using Freecon.Client.Core.States;
using Freecon.Client.Config;
using Freecon.Core.Utils;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Core.States.Components;
using System.Collections.Generic;
using System.Diagnostics;
using Freecon.Client.Core.Services;
using Freecon.Client.Core.Managers;
using Freecon.Client.GUI;
using Freecon.Client.GameStates.StateManagers;
using Freecon.Client.Core.Factories;
using Freecon.Client.Managers.Space;
using Freecon.Client.Managers.States;
using Freecon.Client.Core.Utils;
using Freecon.Client.ViewModel;
using System.Windows.Forms;

namespace Freecon.Client.Xna.Windows
{
    public class WindowsMain : Game
    {
        protected int _bloomSettingsIndex;

        protected IContainer _container;

        protected ClientEnvironmentType _environment;

        protected GraphicsDeviceManager _graphics;

        protected MainManager _mainManager;

        protected SpriteBatch _spriteBatch;

        public static PlayerShipManager playerShipManager;

        protected LoginConfig _LoginConfig;

        protected bool _loadGraphics = true;
        protected bool WindowSizeIsBeingChanged;

        public WindowsMain(ClientEnvironmentType environment)
        {
            _environment = environment;

            //ProtobufMappingSetup.Setup();

            ClientLogger.init(); // Initializes logger
            ClientLogger.Log(Log_Type.INFO, "Game Initialized");
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _LoginConfig = new LoginConfig();

            this.Exiting += Main_Exiting;
        }

       

        void Main_Exiting(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }
        
        protected override void Initialize()
        {
            ConsoleMover.SetConsolePosition(675, 340);

            Window.Title = @"Freecon Galactic: Alpha"; // Sets title for titlebar

            Window.AllowUserResizing = true; // Allows user to resize window
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            Form form = (Form)Control.FromHandle(Window.Handle);// Maximizing the window does not trigger Window.ClientSizeChanged
            form.Resize += Form_Resize;

            IsMouseVisible = true; // Makes mouse visible upon mouse over

            IsFixedTimeStep = true;

            IsMouseVisible = true;
            
            Console.SetWindowSize(80, 25);
            Console.SetWindowPosition(0, 0);

            base.Initialize();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            Window_ClientSizeChanged(sender, e);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var settings = new SettingsManager();

            // Changes size of GameWindow.
            // Todo: Clean this up.
            var m = new RenderingMath(_graphics);
            m.InitGraphicsMode(
                SettingsManager.GameWidth,
                SettingsManager.GameHeight,
                SettingsManager.Fullscreen,
                true
            );

            var bloom = new BloomComponent(Content, _spriteBatch);

            _bloomSettingsIndex = (_bloomSettingsIndex + 1)
                                  % BloomSettings.PresetSettings.Length;

            bloom.Settings = BloomSettings.PresetSettings[6];

            ClientLogger.Log(Log_Type.INFO, "Assets Loaded");

            #region Autofac (disabled, couldn't get it to work, sorry Free!)
            /*


            //HudManager.Initialize(Content, spriteBatch, GraphicsDevice); // Just Chatbox and Radar

            //_UI.OnDocumentCompleted += OnDocumentCompleted;

            // Included so that it forces the assemblies to load into AutoFac
            var whatever = typeof(MainManager);
            var whatever2 = typeof(GameStateManager);
            var whatever3 = typeof(GlobalGameWebLayer);
            var whatever4 = typeof(PlanetStateManager);

            // Todo: Gut this in favor of reactive flow.
            var _bus = BusSetup.StartWith<Conservative>()
                               .Apply<FlexibleSubscribeAdapter>(a => a.ByInterface(typeof(IHandle<>)))
                               .Construct();

            // Create your builder.
            var builder = new ContainerBuilder();


            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                .Where(p => 
                    (p.Name.Contains("Manager") || p.Name.Contains("Factory") || p.Name.Contains("Service") || p.Name.Contains("WebLayer")) &&
                    p.Name != "PhysicsManager")
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                .Where(p => p.Name.Contains("Manager") || p.Name.Contains("Factory") || p.Name.Contains("Service") || p.Name.Contains("WebLayer"))
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            //builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
            //    .Where(p => p is IGameState)               
            //    .SingleInstance();


            //Configure single instances, overriding .InstancePerDependency
            builder.RegisterType<MainNetworkingManager>().SingleInstance();
            builder.RegisterType<NetworkingService>().SingleInstance();
            builder.RegisterType<ClientManager>().SingleInstance();
            builder.RegisterType<MessageService>().SingleInstance();
            builder.RegisterType<PlayerShipManager>().SingleInstance();//PlayerShip is currently only sent on login
            builder.RegisterType<KeyboardManager>().SingleInstance();
            builder.RegisterType<textDrawingService>().SingleInstance();


            

            builder.RegisterInstance<LoginConfig>(new LoginConfig()).SingleInstance();

            var uiPath = Directory.GetCurrentDirectory();

            switch (_environment)
            {
                case ClientEnvironmentType.Development:
                    builder.RegisterInstance<IClientWebViewConfig>(new DebugClientWebViewConfig()).SingleInstance();
                    break;

                default:
                    throw new Exception("You should add a production config, yo!");
            }

            // XNA classes
            builder.RegisterInstance<SpriteBatch>(_spriteBatch).SingleInstance();
            builder.RegisterInstance<GraphicsDeviceManager>(_graphics).SingleInstance();
            builder.RegisterInstance<GraphicsDevice>(_graphics.GraphicsDevice).SingleInstance();
            builder.RegisterInstance<ContentManager>(Content).SingleInstance();
            builder.RegisterInstance<SettingsManager>(settings).SingleInstance();
            builder.RegisterInstance<Game>(this).SingleInstance();
            builder.RegisterInstance<GameWindow>(GameWindow).SingleInstance();

            builder.RegisterInstance<Random>(new Random(8)).SingleInstance();
            builder.RegisterInstance<BloomComponent>(bloom).SingleInstance();
            builder.RegisterInstance<IBus>(_bus).SingleInstance();
            builder.RegisterInstance<WindowsMain>(this).SingleInstance();
            builder.RegisterInstance<IClientLogger>(new FakeLogger()).SingleInstance();

            builder.RegisterType<MainManager>().AsSelf();

            //builder.RegisterType<PlanetStateManager>().SingleInstance();
            //builder.RegisterType<SpaceStateManager>().SingleInstance();
            //builder.RegisterType<PortStateManager>().SingleInstance();
            //builder.RegisterType<LoginStateManager>().SingleInstance();
            object scope1 = new object();


            builder.RegisterType<PhysicsManager>().InstancePerLifetimeScope();


            _container = builder.Build();


          
            
            List<GameStateType> PlayableGameStateScopes = new List<GameStateType> { GameStateType.Planet, GameStateType.Space, GameStateType.Port };


            ContainerBuilder b1 = new ContainerBuilder();

            PlanetStateManager planetsm;
            using(var scope = _container.BeginLifetimeScope("scope1"))
            {
                //var physm = scope.Resolve<PhysicsManager>();
                //builder.RegisterInstance<PhysicsManager>(physm).AsSelf().SingleInstance();
                planetsm = scope.Resolve<PlanetStateManager>();
            }
            b1.RegisterInstance<PlanetStateManager>(planetsm).SingleInstance();

            SpaceStateManager spacesm;
            using (var scope = _container.BeginLifetimeScope("scope2"))
            {
                spacesm = scope.Resolve<SpaceStateManager>();
            }
            b1.RegisterInstance<SpaceStateManager>(spacesm).SingleInstance().ExternallyOwned();

            PortStateManager portsm;
            using (var scope = _container.BeginLifetimeScope("scope3"))
            {
                portsm = scope.Resolve<PortStateManager>();
            }
            b1.RegisterInstance<PortStateManager>(portsm).SingleInstance();


            
            b1.Update(_container);
            
            //var container2 = b1.Build();
            using (var scope = _container.BeginLifetimeScope("scope4"))
            {
                _mainManager = _container.Resolve<MainManager>();
                //_container.Resolve<GameStateNetworkingManager>();
            }

             */
             
            #endregion
            
            _mainManager = _manualBuild();


        }

        protected virtual MainManager _manualBuild()
        {            
         
            #region Singletons
            ClientManager clientManager = new ClientManager(new CoreNetworkConfig());
            CameraService cameraService = new CameraService();

            ParticleManager particleManager = null;
            TextDrawingService textDrawingService = null;
            TextureManager textureManager = null;
            BloomComponent bloom = null;
            if (_loadGraphics)
            {
                
                textureManager = new TextureManager(Content);
                particleManager = new ParticleManager(_graphics, _spriteBatch, Content, textureManager);
                textDrawingService = new TextDrawingService(textureManager.DefaultDrawFont, _spriteBatch);
                bloom = new BloomComponent(Content, _spriteBatch);
            }

            
            LidgrenMessenger messenger = new LidgrenMessenger(clientManager);
            MessageService_ToServer messageService = new MessageService_ToServer(messenger);
            IClientWebViewConfig clientWebviewConfig = null;
            switch (_environment)
            {
                case ClientEnvironmentType.Development:
                    clientWebviewConfig = new DebugClientWebViewConfig();
                    break;

                default:
                    throw new Exception("You should add a production config, yo!");
            }

            
            
            var chatManager = new NewChatManager(clientManager);

            LidgrenNetworkingService networkingService = new LidgrenNetworkingService();

            GlobalGameUISingleton globalGameUiSingleton = new GlobalGameUISingleton(clientWebviewConfig, GraphicsDevice, Window, _spriteBatch);


            GlobalGameWebLayer globalGameWebLayer = new GlobalGameWebLayer(clientWebviewConfig, new GameInterfaceWebView(globalGameUiSingleton, chatManager, new GameInterfaceViewModel()));

            // Force the static to initialize now
            Utilities.NextUnique();

            #endregion

            List<IGameState> gameStates = new List<IGameState>();

            gameStates.Add(_buildPlanetStateManager(globalGameUiSingleton, cameraService, textureManager, particleManager, textDrawingService, messageService, networkingService));
            gameStates.Add(_buildSpaceStateManager(globalGameUiSingleton, cameraService, textureManager, particleManager, textDrawingService, messageService, clientWebviewConfig, networkingService, bloom));
            //gameStates.Add(_buildMoonStateManager(chatManager, CefSharpViewFactory, cameraService, textureManager, particleManager, textDrawingService, messageService, clientWebviewConfig, networkingService));
            gameStates.Add(_buildPortStateManager(globalGameUiSingleton, messageService, networkingService));
            gameStates.Add(_buildLoginStateManager(globalGameUiSingleton, clientManager, messageService, networkingService));
            gameStates.Add(_buildColonyStateManager(globalGameUiSingleton, messageService, networkingService));
            GameStateManager gameStateManager = BuildGameStateManager(cameraService, gameStates, textDrawingService, globalGameWebLayer);
            MainNetworkingManager mainNetworkingManager = new MainNetworkingManager(chatManager, clientManager, networkingService);
            
            return new MainManager(bloom, textDrawingService, this, gameStateManager, _graphics, mainNetworkingManager);

            
        }

        

        DrawablePlanetStateManager _buildPlanetStateManager(GlobalGameUISingleton globalGameUiSingleton, CameraService cameraService, TextureManager textureManager, ParticleManager particleManager, TextDrawingService textDrawingService, MessageService_ToServer messageService, LidgrenNetworkingService networkingService)
        {
            CollisionManager instance_collisionManager;
            ParticleManager instance_particleManager = null;
            SimulationManager instance_simulationManager;
            TargetingService instance_targetingService;
            TeamManager instance_teamManager;
            UIConversionService instance_uiConversionService;
            ClientShipManager instance_clientShipManager;
            PhysicsManager instance_physicsManager;
            ProjectileManager instance_projectileManager;
            StructureFactoryManager instance_structureFactoryManager;
            WarpHoleManager instance_warpholeManager;
            SelectionManager instance_selectionManager;
            FloatyAreaObjectManager instance_floatyAreaObjectManager;

            
       
            instance_collisionManager = new CollisionManager(messageService);
            if (_loadGraphics)
            {
                instance_particleManager = new ParticleManager(_graphics, _spriteBatch, Content, textureManager);
            }

            instance_simulationManager = new SimulationManager();
            instance_targetingService = new TargetingService();
            instance_teamManager = new TeamManager(instance_targetingService);
            PlayerShipManager instance_playerShipManager = new PlayerShipManager(messageService);
            IClientPlayerInfoManager instance_clientPlayerInfoManager = new PlayablePlayerInfoManager(instance_playerShipManager);
            instance_uiConversionService = new UIConversionService(cameraService, instance_playerShipManager, _spriteBatch);
            instance_physicsManager = new PhysicsManager();
            instance_projectileManager = new ProjectileManager(instance_particleManager, instance_physicsManager.World, _spriteBatch, instance_targetingService, instance_simulationManager, messageService, instance_collisionManager);
            instance_clientShipManager = new ClientShipManager(instance_particleManager, instance_playerShipManager, _spriteBatch, textureManager, instance_simulationManager, instance_targetingService, instance_teamManager, instance_projectileManager, messageService, instance_clientPlayerInfoManager, false);
            instance_structureFactoryManager = new StructureFactoryManager(messageService, instance_physicsManager.World, instance_projectileManager, instance_targetingService, instance_teamManager, textureManager, instance_clientShipManager, _spriteBatch, false);
            instance_warpholeManager = new WarpHoleManager(messageService, instance_particleManager, instance_physicsManager, instance_clientShipManager, _loadGraphics?textureManager.Warphole:null);
            instance_selectionManager = new SelectionManager(textDrawingService, _spriteBatch, instance_clientShipManager, messageService, instance_physicsManager, instance_playerShipManager, instance_targetingService, instance_uiConversionService);
            instance_floatyAreaObjectManager = new FloatyAreaObjectManager(instance_physicsManager.World, textureManager, messageService, _spriteBatch, particleManager);

            PhysicsConfig physicsConfig = new PhysicsConfig();

            return new DrawablePlanetStateManager(
                _spriteBatch,
                instance_clientPlayerInfoManager,
                instance_collisionManager,
                globalGameUiSingleton,
                instance_particleManager,
                instance_physicsManager,
                instance_playerShipManager,
                instance_projectileManager,
                instance_clientShipManager,
                instance_structureFactoryManager,
                textureManager,
                networkingService,
                instance_selectionManager,
                instance_simulationManager,
                instance_targetingService,
                instance_teamManager,
                GameStateType.Planet,
                instance_uiConversionService,
                instance_warpholeManager,
                Window,
                instance_floatyAreaObjectManager,
                messageService,
                physicsConfig.PlanetTileWidth,
                physicsConfig.PlanetTileHeight);            

        }
     
        DrawableSpaceStateManager _buildSpaceStateManager(GlobalGameUISingleton globalGameUiSingleton, CameraService cameraService, TextureManager textureManager, ParticleManager particleManager, TextDrawingService textDrawingService, MessageService_ToServer messageService, IClientWebViewConfig clientWebviewConfig, LidgrenNetworkingService networkingService, BloomComponent bloom)
        {

            CollisionManager instance_collisionManager;
            ParticleManager instance_particleManager = null;
            SimulationManager instance_simulationManager;
            TargetingService instance_targetingService;
            TeamManager instance_teamManager;
            UIConversionService instance_uiConversionService;
            ClientShipManager instance_clientShipManager;
            PhysicsManager instance_physicsManager;
            ProjectileManager instance_projectileManager;
            StructureFactoryManager instance_structureFactoryManager;
            WarpHoleManager instance_warpholeManager;
            SelectionManager instance_selectionManager;
            FloatyAreaObjectManager instance_floatyAreaObjectManager;

            //Space unique
            BackgroundManager instance_backgroundManager = null;
            SpaceManager instance_spaceManager;
            BorderManager instance_borderManager = null;
            GravityManager instance_gravityManager;
            SpaceObjectManager instance_spaceObjectManager;
            
            
            instance_collisionManager = new CollisionManager(messageService);
            if (_loadGraphics)
            {
                instance_particleManager = new ParticleManager(_graphics, _spriteBatch, Content, textureManager);
            }
            instance_simulationManager = new SimulationManager();
            instance_targetingService = new TargetingService();
            instance_teamManager = new TeamManager(instance_targetingService);
            PlayerShipManager instance_playerShipManager = new PlayerShipManager(messageService);
            IClientPlayerInfoManager instance_clientPlayerInfoManager = new PlayablePlayerInfoManager(instance_playerShipManager);
            instance_uiConversionService = new UIConversionService(cameraService, instance_playerShipManager, _spriteBatch);
            instance_physicsManager = new PhysicsManager();
            instance_projectileManager = new ProjectileManager(instance_particleManager, instance_physicsManager.World, _spriteBatch, instance_targetingService, instance_simulationManager, messageService, instance_collisionManager);
            instance_clientShipManager = new ClientShipManager(instance_particleManager, instance_playerShipManager, _spriteBatch, textureManager, instance_simulationManager, instance_targetingService, instance_teamManager, instance_projectileManager, messageService, instance_clientPlayerInfoManager, false);
            instance_structureFactoryManager = new StructureFactoryManager(messageService, instance_physicsManager.World, instance_projectileManager, instance_targetingService, instance_teamManager, textureManager, instance_clientShipManager, _spriteBatch, false);
            instance_warpholeManager = new WarpHoleManager(messageService, instance_particleManager, instance_physicsManager, instance_clientShipManager, _loadGraphics ? textureManager.Warphole : null);
            instance_selectionManager = new SelectionManager(textDrawingService, _spriteBatch, instance_clientShipManager, messageService, instance_physicsManager, instance_playerShipManager, instance_targetingService, instance_uiConversionService);

            if (_loadGraphics)
            {               
                instance_backgroundManager = new BackgroundManager(Content, instance_particleManager, _spriteBatch, cameraService,
                    new Random(8));

                
            }

            instance_borderManager = new BorderManager(_loadGraphics?textureManager.tex_DotW:null, _spriteBatch,
                    instance_physicsManager);

            instance_gravityManager = new GravityManager(instance_physicsManager);
            instance_spaceObjectManager = new SpaceObjectManager(textureManager, messageService, _spriteBatch, instance_particleManager, instance_physicsManager);
            instance_spaceManager = new SpaceManager(_spriteBatch, instance_borderManager, instance_gravityManager, instance_physicsManager, instance_spaceObjectManager, instance_warpholeManager);
            instance_floatyAreaObjectManager = new FloatyAreaObjectManager(instance_physicsManager.World, textureManager, messageService, _spriteBatch, particleManager);

            return new DrawableSpaceStateManager(
                messageService,
                instance_clientPlayerInfoManager,
                instance_backgroundManager,
                bloom,
                instance_collisionManager,
                this,
                Window,
                globalGameUiSingleton,
                networkingService,
                _spriteBatch,
                instance_particleManager,
                instance_physicsManager,
                instance_playerShipManager,
                instance_projectileManager,
                instance_selectionManager,
                instance_simulationManager,
                instance_clientShipManager,
                instance_spaceManager,
                instance_structureFactoryManager,
                textureManager,
                instance_targetingService,
                instance_teamManager,
                instance_warpholeManager,
                instance_spaceObjectManager,
                instance_uiConversionService,
                instance_floatyAreaObjectManager);
       
        }

        DrawableColonyStateManager _buildColonyStateManager(GlobalGameUISingleton globalGameUiSingleton, MessageService_ToServer messageService, LidgrenNetworkingService networkingService)
        {

            IClientPlayerInfoManager instance_clientPlayerInfoManager;

            instance_clientPlayerInfoManager = new NonPlayablePlayerInfoManager();
            return new DrawableColonyStateManager(null, globalGameUiSingleton, networkingService, instance_clientPlayerInfoManager, _spriteBatch, messageService, Window);
        }

        DrawablePortStateManager _buildPortStateManager(GlobalGameUISingleton globalGameUiSingleton, MessageService_ToServer messageService, LidgrenNetworkingService networkingService)
        {
            IClientPlayerInfoManager clientPlayerInfoManager = new NonPlayablePlayerInfoManager();
            return new DrawablePortStateManager(null, clientPlayerInfoManager, networkingService, globalGameUiSingleton, _spriteBatch, messageService);            
        }

        protected LoginStateManager _buildLoginStateManager(GlobalGameUISingleton globalGameUiSingleton, ClientManager clientManager, MessageService_ToServer messageService, LidgrenNetworkingService networkingService)
        {
            return new LoginStateManager(_LoginConfig, clientManager, globalGameUiSingleton, messageService, networkingService);              
        }

        protected virtual GameStateManager BuildGameStateManager(CameraService cameraService,
            IEnumerable<IGameState> gameStates, TextDrawingService textDrawingService,
            GlobalGameWebLayer globalGameWebLayer)
        {
            return new GameStateManager(cameraService, gameStates, textDrawingService, globalGameWebLayer);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {              
            _mainManager.Update(new XNAGameTimeService(gameTime));
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _mainManager.Draw();
        }

        public void SetupServiceLayer()
        {
            var tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(
                ServiceLayerRunning,
                TaskCreationOptions.LongRunning,
                tokenSource.Token
            ); 

            this.Exiting += (sender, e) =>
            {
                tokenSource.Cancel();
            };
        }

        private void ServiceLayerRunning(object asdf)
        {

        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (WindowSizeIsBeingChanged)//ApplyChanges calls the event again, this avoids stackoverflow
                return;

            WindowSizeIsBeingChanged = true;
            
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
            WindowSizeIsBeingChanged = false;            
            
        }


    }
}
