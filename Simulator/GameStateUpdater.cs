using System;
using System.Timers;
using Core.Interfaces;
using Freecon.Client.GameStates;
using Server.Managers;

namespace Simulator
{
    public class GameStateUpdater
    {
        PlayableGameState _gameState;

        public Timer UpdateTimer;

        private IGameTimeService _gameTimeService;

        public GameStateUpdater(PlayableGameState gameState, IGameTimeService gameTimeService, float updateInterval)
        {
            _gameState = gameState;
            _gameTimeService = gameTimeService;
            UpdateTimer = new Timer(updateInterval);
            UpdateTimer.Elapsed += Update;
            UpdateTimer.AutoReset = false;//DO NOT SET THIS TO TRUE, OTHERWISE EVENT MIGHT FIRE BEFORE PREVIOUS EVENT COMPLETES AND WE'LL GET CONCURRENCY ERRORS
            UpdateTimer.Start();

        }

        void Update(object sender, EventArgs e)
        {
            if (_gameState.IsUpdating)
                return;

            try
            {
                _gameState.StateWillUpdate(_gameTimeService);

                foreach (var updatable in _gameState.AsynchronousUpdateList)
                {
                    // Async, but not awaited, order shouldn't matter.
                    updatable.Update(_gameTimeService);
                }

                // Update current state
                foreach (var updatable in _gameState.SynchronousUpdateList)
                {
                    updatable.Update(_gameTimeService);
                }

                _gameState.StateDidUpdate(_gameTimeService);
                
            }
            catch (Exception ee)
            {
                ConsoleManager.WriteLine("Error in Simulator: " + ee.Message, ConsoleMessageType.Error);
                ConsoleManager.WriteLine(ee.StackTrace, ConsoleMessageType.Error);
                _gameState.IsUpdating = false;
            }

            UpdateTimer.Start();
        }

    }
}
