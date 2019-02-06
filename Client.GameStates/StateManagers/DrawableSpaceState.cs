using Freecon.Client.Core.Interfaces;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Managers.Space;
using Freecon.Client.Mathematics;
using Freecon.Client.Mathematics.Effects;
using Freecon.Client.Objects.Structures;
using Core.Models.Enums;
using Freecon.Client.Core.Factories;
using Freecon.Client.Core.Managers;
using Freecon.Client.Core.Services;
using Freecon.Client.Core.States.Components;
using Freecon.Client.View;
using Freecon.Client.View.CefSharp;
using Freecon.Client.View.Xna;
using Freecon.Client.ViewModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Freecon.Client.Managers.States
{
    public class DrawableSpaceStateManager : SpaceStateManager, IDrawableGameState
    {
        BloomComponent _bloom;
        SpriteBatch _spriteBatch;
        UIConversionService _uiConversionService;
        ParticleManager _particleManager;
        TextureManager _textureManager;
        SpaceGameView _spaceGameView;

        protected HashSet<IDraw> _drawList { get; set; }
        public virtual IEnumerable<IDraw> DrawList
        {
            get { return _drawList; }
        }

        public Camera2D Camera { get; protected set; }

        public DrawableSpaceStateManager(
            MessageService_ToServer messageService,
            IClientPlayerInfoManager clientPlayerInfoManager,
            BackgroundManager backgroundManager,
            BloomComponent bloom,
            CollisionManager collisionManager,
            Game game,
            GameWindow window,
            GlobalGameUISingleton globalGameUiSingleton,
            LidgrenNetworkingService networkingService,
            SpriteBatch spriteBatch,
            ParticleManager particleManager,
            PhysicsManager physicsManager,
            PlayerShipManager playerShipManager,
            ProjectileManager projectileManager,
            SelectionManager selectionManager,
            SimulationManager simulationManager,
            ClientShipManager clientShipManager,
            SpaceManager spaceManager,
            StructureFactoryManager structureFactoryManager,
            TextureManager textureManager,
            TargetingService targetingService,
            TeamManager teamManager,
            WarpHoleManager warpHoleManager,
            SpaceObjectManager spaceObjectManager,
            UIConversionService uiConversionService,
            FloatyAreaObjectManager floatyAreaObjectManager)
            : base(null, clientPlayerInfoManager, globalGameUiSingleton,
                collisionManager, networkingService, physicsManager, playerShipManager,
                projectileManager, selectionManager, simulationManager, clientShipManager, spaceManager, structureFactoryManager, targetingService,
                teamManager, warpHoleManager, messageService, floatyAreaObjectManager)
        {
            _bloom = bloom;
            _spriteBatch = spriteBatch;
            _textureManager = textureManager;
            _uiConversionService = uiConversionService;
            _spriteBatch = spriteBatch;
            _particleManager = particleManager;


            Camera = new Camera2D(window);
            Camera.Zoom = 1f;

            //ViewModel = new SpaceViewModel(ClientShipManager);
            _spaceGameView = new SpaceGameView(
                messageService,
                backgroundManager,
                bloom,
                Camera,
                particleManager,
                projectileManager,
                clientShipManager,
                spaceManager,
                spaceObjectManager,
                spriteBatch,
                uiConversionService,
                floatyAreaObjectManager,
                ViewModel,
                globalGameUiSingleton.GameUI);
            
            _viewUpdateList.Add(_spaceGameView);

            _drawList = new HashSet<IDraw>();
            _drawList.Add(_spaceGameView);
            
            _synchronousUpdateList.Add(_particleManager);
        }
        public virtual void StateWillDraw(Camera2D camera)
        {
            Debugging.textDrawingService.DrawTextToScreenRight(5, SystemName);

        }

        public virtual void StateDidDraw(Camera2D camera)
        {

        }

        public override void KillStructure(int ID)
        {

            Structure structure = StructureManager.GetStructureByID(ID);
            if (structure != null)
            {
                Vector2 effectPos = new Vector2(structure.xPos, structure.yPos);
                _particleManager?.TriggerEffect(effectPos, ParticleEffectType.ExplosionEffect, 5);
                base.KillStructure(ID);
            }


        }
         
        public override void Activate(IGameState previous)
        {
            base.Activate(previous);

            if(_playerShipManager.PlayerShip != null)
            {
                _playerShipManager.PlayerShip.EnterMode = false;
            }
        }

        public override void Clear()
        {
            base.Clear();
            _particleManager?.Reset();
            
        }
    }
}
