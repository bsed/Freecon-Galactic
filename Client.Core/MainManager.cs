using Microsoft.Xna.Framework;
using Core.Interfaces;
using Freecon.Client.GUI;
using Freecon.Client.Managers;
using Freecon.Client.Mathematics.Effects;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.States;
using Freecon.Core.Utils;

namespace Freecon.Client.Base
{
    public class MainManager
    {
        TextDrawingService _textDrawingService;

        GraphicsDeviceManager _graphics;

        MainNetworkingManager _mainNetworkingManager;

        public Game _game;

        IGameStateManager _gameStateManager;

        private OPSCounter _drawsPerSecondCounter;
        private OPSCounter _updatesPerSecondCounter;

        public MainManager(BloomComponent bloom,
                           TextDrawingService textDrawingService,
                           Game game,
                           IGameStateManager gameStateManager,
                           GraphicsDeviceManager graphics,
                           MainNetworkingManager mainNetworkingManager)
        {
         
            _textDrawingService = textDrawingService;
            
            _game = game;

            _gameStateManager = gameStateManager;

            _graphics = graphics;

            _mainNetworkingManager = mainNetworkingManager;

            _drawsPerSecondCounter = new OPSCounter(5);
            _updatesPerSecondCounter = new OPSCounter(5);
        }

        public void Update(IGameTimeService gameTime)
        {
            MouseManager.Update(_game.IsActive); // Gets current mouse state.
            KeyboardManager.SetCurrentKeys(_game.IsActive); // Gets current keyboard state.
            GamepadManager.SetCurrentButtons(_game.IsActive); // Gets current controller state.

            _mainNetworkingManager.Update(gameTime);
            if(_mainNetworkingManager.ChangeGameState)
            {
                _gameStateManager.SetState(_mainNetworkingManager.NextState);
                _mainNetworkingManager.ChangeGameState = false;

            }

            _updatesPerSecondCounter.Update();
            _gameStateManager.Update(gameTime);

            _textDrawingService.Update(gameTime);

            switch(_gameStateManager.ActiveState.StateType)
            {
                case GameStateType.Login:
                    {
                        //_gameStateManager.
                        break;
                    }


            }
                      
                        
            KeyboardManager.SetOldKeys(_game.IsActive); // Sets old keystate for next iteration.    
            GamepadManager.SetOldButtons(_game.IsActive); // Sets old keystates for next iteration for gamepad.
        }

        public void Draw()
        {
            _graphics.GraphicsDevice.Clear(Color.Black);

            // Why the fuck does everything want a camera to draw?
            _gameStateManager.Draw(null);

            _drawsPerSecondCounter.Update();

            _textDrawingService.DrawTextToScreenRight(0, "FPS: " + _drawsPerSecondCounter.GetAverageOPS());
            _textDrawingService.DrawTextToScreenRight(1, "UPS: " + _updatesPerSecondCounter.GetAverageOPS());
        }
    }
}
