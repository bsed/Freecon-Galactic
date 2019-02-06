using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.Interfaces.States;
using Freecon.Client.Core.Services;
using Freecon.Client.Core.States.Components;
using Core.Interfaces;
using Freecon.Client.GUI;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using System;
using System.Collections.Generic;
using Freecon.Client.GameStates;
using Freecon.Client.Mathematics;
using Freecon.Client.View;
using Freecon.Client.View.CefSharp;

namespace Freecon.Client.Core.States
{
    /// <summary>
    /// Manages the active state and handles logic flow for IGameState instances.
    /// </summary>
    public class GameStateManager : ISynchronousManager, IGameStateManager
    {
        protected CameraService _cameraService;
        protected TextDrawingService _textDrawingService;

        protected readonly GlobalGameWebLayer _globalGameWebLayer;

        public IGameState ActiveState { get { return _activeState; } }
        public IGameState _activeState { get; protected set; }

        public IGameState NextState { get { return _nextState; } }
        public IGameState _nextState { get; private set; }

        private IHasGameWebView _nextWebView { get; set; }

        Dictionary<GameStateType, IGameState> _gameStates;

        public bool IsPlayableGameState
        {
            get
            {
                return _activeState != null && _activeState is IPlayableGameState;
            }
        }

        bool _pendingStateChange { get; set; }

        public GameStateManager(
            CameraService cameraService,
            IEnumerable<IGameState> gameStates,
            TextDrawingService textDrawingService,
            GlobalGameWebLayer globalGameWebLayer)
        {
            _cameraService = cameraService;
            _textDrawingService = textDrawingService;

            _gameStates = new Dictionary<GameStateType, IGameState>();
            _globalGameWebLayer = globalGameWebLayer;

            _globalGameWebLayer.Activate();

            foreach (var gsm in gameStates)
            {
                _gameStates.Add(gsm.StateType, gsm);

            }
            
            SetState(GameStateType.Login);
        }

        public void SetState(GameStateType type)
        {
            var previousState = _activeState;
            _nextState = _gameStates[type];

            if (_nextState == previousState || _nextState == null)
            {
                return;
            }
            
            if (previousState != null && previousState.Status != GameStateStatus.Inactive)
            {
                previousState.Deactivate(_nextState);

                var previousWebView = previousState as IHasWebGameState;

                previousWebView?.RawGameWebView.Unregister();
            }

            var nextWebGameState = _nextState as IHasWebGameState;
            _nextWebView = nextWebGameState?.RawGameWebView;

            // If they are not (de)activating, tell states to (de)activate
            if (_nextState.Status != GameStateStatus.Active)
            {
                _nextState.Activate(previousState);
                _nextWebView?.Register();
                _pendingStateChange = true;
            }
            
            if (_nextState.Status == GameStateStatus.Active && (previousState == null || previousState.Status == GameStateStatus.Inactive))
            {
                _activeState = _nextState;
                _cameraService.SetActiveState(_activeState);
                _nextState = null;
                _pendingStateChange = false;

                // Propagate state change to UI
                _globalGameWebLayer?.SetUIStateManagerContainer(_activeState.GetStateManagerContainer());
            }
        }

                        
        public void Update(IGameTimeService gameTime)
        {
            _activeState.StateWillUpdate(gameTime);

            // Update current state
            foreach (var updatable in _activeState.AsynchronousUpdateList)
            {
                // Async, but not awaited, order shouldn't matter.
                updatable.Update(gameTime);
            }

            // Update current state
            foreach (var updatable in _activeState.SynchronousUpdateList)
            {
                updatable.Update(gameTime);
            }

            // Update view state
            foreach (var updatable in _activeState.ViewUpdateList)
            {
                updatable.Update(gameTime);
            }

            _nextWebView.Update(gameTime);

            _globalGameWebLayer.Update(gameTime);

            _activeState.StateDidUpdate(gameTime);

            // If there is a pending state change, call setState until NextState sets status to Active            
            if(_pendingStateChange)
            {
                SetState(_nextState.StateType);
            }

#if DEBUG
            Debugging.Update();
#endif
        }

        public virtual void Draw(Camera2D camera)
        {
            var activeState = _activeState as IDrawableGameState;//I know, you're nauseous, please don't be mad. It'll be ok. No one will ever notice. Our little secret. I promise. xoxo

            if (activeState != null)
            {
                
                activeState.StateWillDraw(activeState.Camera);

                foreach (var drawable in activeState.DrawList)
                {
                    drawable.Draw(activeState.Camera);
                }

                _nextWebView.Draw(camera);

                _globalGameWebLayer.Draw(camera);

                activeState.StateDidDraw(activeState.Camera);

                _textDrawingService.Draw();
            }

            
        }
    }
}
