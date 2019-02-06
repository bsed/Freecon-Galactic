using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Client.Objects;
using Freecon.Client.Core.Factories;
using Freecon.Client.Core.Managers;
using Freecon.Client.Core.Services;
using Freecon.Client.Core.States;
using Freecon.Client.Core.States.Components;
using Freecon.Client.View;
using Freecon.Client.View.CefSharp;
using Freecon.Client.View.Xna;
using Freecon.Client.ViewModel;
using Freecon.Core.Networking.Interfaces;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Freecon.Client.Managers.Networking;

namespace Freecon.Client.Managers.States
{
    
    public class DrawablePlanetStateManager : PlanetStateManager, IDrawableGameState
    {
        protected PlanetGameView _planetGameView;
        protected ParticleManager _particleManager;

        protected SpriteBatch _spriteBatch;
        protected TextureManager _textureManager;

        public IEnumerable<IDraw> DrawList { get { return _drawList; } }

        HashSet<IDraw> _drawList;

        public Camera2D Camera { get; protected set; }

        
        public DrawablePlanetStateManager(
            SpriteBatch spriteBatch,
            IClientPlayerInfoManager clientPlayerInfoManager,
            CollisionManager collisionManager,
            GlobalGameUISingleton globalGameUiSingleton,
            ParticleManager particleManager,
            PhysicsManager physicsManager,
            PlayerShipManager playerShipManager,
            ProjectileManager projectileManager,
            ClientShipManager clientShipManager,
            StructureFactoryManager structureFactoryManager,
            TextureManager textureManager,
            INetworkingService networkingService,
            SelectionManager selectionManager,
            SimulationManager simulationManager,
            TargetingService targetService,
            TeamManager teamManager,
            GameStateType stateType,//Should be planet or moon
            UIConversionService uiConversionService,
            WarpHoleManager warpholeManager,
            GameWindow window,
            FloatyAreaObjectManager floatyAreaObjectManager,
            MessageService_ToServer messageService,
            int tileWidth, int tileHeight)
            : base(null,
                clientPlayerInfoManager,
                globalGameUiSingleton,
                collisionManager,
                physicsManager,
                playerShipManager,
                projectileManager,
                clientShipManager,
                structureFactoryManager,
                warpholeManager,
                networkingService,
                selectionManager,
                simulationManager,
                targetService,
                teamManager,
                stateType,
                floatyAreaObjectManager,
                messageService,
                tileWidth,
                tileHeight)
        {
            _spriteBatch = spriteBatch;
            _textureManager = textureManager;
            _particleManager = particleManager;
            Camera = new Camera2D(window);
            Camera.Zoom = 1f;
            //ViewModel = new PlanetViewModel(clientShipManager);
            _planetGameView = new PlanetGameView(
                Camera,
                uiConversionService,
                particleManager,
                ViewModel,
                projectileManager,
                clientShipManager,
                spriteBatch,
                textureManager,
                floatyAreaObjectManager,
                globalGameUiSingleton.GameUI,
                SendStructurePlacementRequest);

            _spriteBatch = spriteBatch;

            globalGameUiSingleton.GameUI.RegisterCallbackVoid("SendStructurePlacementRequest", SendStructurePlacementRequest);

            _viewUpdateList.Add(_planetGameView);

            _drawList = new HashSet<IDraw>();
            _drawList.Add(_planetGameView);

            
            
            _synchronousUpdateList.Add(_particleManager);
            
        }
              
        public void StateWillDraw(Camera2D camera)
        {

        }

        public void StateDidDraw(Camera2D camera)
        {

        }

        public override void Activate(IGameState previous)
        {
            base.Activate(previous);
            if (_playerShipManager.PlayerShip != null)
            {
                _playerShipManager.PlayerShip.EnterMode = false;
            }
        }
        
        public override void Clear()
        {
            base.Clear();
            ViewModel.Clear();
            _particleManager.Reset();
        }

        public override void LoadPlanetLevel(
            PlanetTypes planetType,
            IEnumerable<IEnumerable<Vector2>> islands,
            int height,
            int width,
            bool[] layoutArray)
        {
            ViewModel.Level = new PlanetLevel(_spriteBatch, _wallWidth, _wallHeight, _physicsManager, planetType, islands, height, width, layoutArray, _textureManager);
        }
    }
}
