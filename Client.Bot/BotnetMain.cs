using System.Collections.Generic;
using System.Timers;
using Client.Bot;
using Freecon.Client.Base;
using Freecon.Client.Core;
using Freecon.Client.Xna.Windows;
using Freecon.Client.Config;
using Freecon.Client.Core.Managers;
using Freecon.Client.Core.Services;
using Freecon.Client.Core.States;
using Freecon.Client.Core.States.Components;
using Freecon.Client.Core.Utils;
using Freecon.Client.GUI;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Mathematics.Effects;
using Freecon.Client.View.CefSharp;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Core.Configs;
using Freecon.Client.StateBuilders;

namespace Freecon.Client.Bot
{
    class BotnetMain:WindowsMain
    {
        private Timer _updateTimer;

        public BotnetMain(ClientEnvironmentType environment) : base(environment)
        {
            _LoginConfig = new LoginConfig {LoginIP = "73.136.102.14"};

            _loadGraphics = false;

            _updateTimer = new Timer(16.66666f);
            _updateTimer.AutoReset = false;

            _updateTimer.Elapsed += _updateTimer_Elapsed;

            Initialize();
            LoadContent();
        }

        private void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _mainManager.Update(new BotnetGametimeService());
            ((Timer) sender).Start();
        }

        public void Start()
        {
            _updateTimer.Start();
        }

        protected override GameStateManager BuildGameStateManager(CameraService cameraService, IEnumerable<IGameState> gameStates,
            TextDrawingService textDrawingService, GlobalGameWebLayer globalGameWebLayer)
        {
            return new BotnetGameStateManager(cameraService, gameStates, textDrawingService, globalGameWebLayer);
        }

        protected override MainManager _manualBuild()
        {

            #region Singletons
            ClientManager clientManager = new ClientManager(new CoreNetworkConfig());
            CameraService cameraService = new CameraService();

            TextDrawingService textDrawingService = null;
            BloomComponent bloom = null;
           


            LidgrenMessenger messenger = new LidgrenMessenger(clientManager);
            MessageService_ToServer messageService = new MessageService_ToServer(messenger);

         

            LidgrenNetworkingService networkingService = new LidgrenNetworkingService();


            // Force the static to initialize now
            Utilities.NextUnique();

            #endregion
            
            List<IGameState> gameStates = new List<IGameState>();

            gameStates.Add(BotStateBuilder.BuildPlanetStateManager(clientManager, networkingService, new PhysicsConfig()));
            gameStates.Add(BotStateBuilder.BuildSpaceStateManager(clientManager, networkingService));
            gameStates.Add(_buildLoginStateManager(null, clientManager, messageService, networkingService));

            NewChatManager chatManager = new NewChatManager(clientManager);

            GameStateManager gameStateManager = BuildGameStateManager(cameraService, gameStates, textDrawingService, null);
            MainNetworkingManager mainNetworkingManager = new MainNetworkingManager(chatManager, clientManager, networkingService);

            return new MainManager(bloom, textDrawingService, this, gameStateManager, _graphics, mainNetworkingManager);


        }

        protected override void LoadContent()
        {
            _mainManager = _manualBuild();
        }


    }
}
