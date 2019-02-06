using System;
using System.Collections.Generic;
using Freecon.Client.GameStates;
using Freecon.Core.Networking.Interfaces;
using RedisWrapper;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Utils;
using Freecon.Client.Config;
using Simulator;
using Freecon.Core.Configs;
using Freecon.Core.Networking.Models;
using Server.Managers;
using ConsoleMessageType = Server.Managers.ConsoleMessageType;
using Freecon.Core.Networking.Models.Simulator;
using Freecon.Client.StateBuilders;

namespace Freecon.Simulator
{
    public class MainManager
    {

        INetworkingService _networkingService;
        RedisServer _redisServer;
        private SimulatorGameTimeService _gameTimeService;

        Dictionary<int, PlayableGameState> _gameStates = new Dictionary<int, PlayableGameState>();//IDs are serverside GalaxyIDs

        Dictionary<int, GameStateUpdater> _areaIDToUpdater = new Dictionary<int, GameStateUpdater>(); 

        private SimulatorConfig _simulatorConfig;
        private PhysicsConfig _physicsConfig;

        private int _simulatorID;

        bool _connectedToServer;

        private float _lastTimeStamp;

        public MainManager(int simulatorID, RedisNetworkingService redisNetworkingService, SimulatorGameTimeService gameTimeService, SimulatorConfig simulatorConfig, PhysicsConfig physicsConfig)
        {
            _simulatorID = simulatorID;
            _redisServer = redisNetworkingService.RedisServer;
            _networkingService = redisNetworkingService;
            _gameTimeService = gameTimeService;
            _simulatorConfig = simulatorConfig;
            _physicsConfig = physicsConfig;

            Utilities.NextUnique();//Force the static to initialize now

            
        }
                
        public void Update(object sender, EventArgs e)
        {
            _gameTimeService.ElapsedMilliseconds = _gameTimeService.TotalMilliseconds - _lastTimeStamp;
            _lastTimeStamp = _gameTimeService.TotalMilliseconds;
        }

        public void StartSimulatingArea(AreaTypes areaType, int areaID)
        {
            if(_gameStates.ContainsKey(areaID))
            {
                throw new Exception("AreaID is already being simulated!");
            }

            PlayableGameState newState;

            switch (areaType)
            {
                case AreaTypes.System:
                    {
                        newState = SimulatorStateBuilder.BuildSpaceStateManager(areaID, _redisServer, _networkingService);

                        break;
                    }
                case AreaTypes.Planet:
                    {
                        newState = SimulatorStateBuilder.BuildPlanetStateManager(areaID, _redisServer, _networkingService, _physicsConfig);
                        break;
                    }


                default:
                    throw new InvalidOperationException(areaType + " not implemeted in the simulator.");

            }

            FinalizeNewState(newState, areaID);


        }

        void FinalizeNewState(PlayableGameState newState, int areaID)
        {

            newState.IncreaseUpdateInterval += _increaseUpdateInterval;
            newState.DecreaseUpdateInterval += _decreaseUpdateInterval;

            _areaIDToUpdater.Add(areaID, new GameStateUpdater(newState, _gameTimeService, _simulatorConfig.EmptyGameStateUpdateIntervalMS));
            newState.Activate(newState);
            
            _gameStates.Add(areaID, newState);

        }

        
        

        void _increaseUpdateInterval(object sender, int areaID)
        {
#if DEBUG
            ConsoleManager.WriteLine("Increasing update interval to " + _simulatorConfig.OccupiedGameStateUpdateIntervalMS);
#endif

            _areaIDToUpdater[areaID].UpdateTimer.Interval = _simulatorConfig.OccupiedGameStateUpdateIntervalMS;
        }

        void _decreaseUpdateInterval(object sender, int areaID)
        {
#if DEBUG
            ConsoleManager.WriteLine("Decreasing update interval to " + _simulatorConfig.EmptyGameStateUpdateIntervalMS);
#endif
            
            _areaIDToUpdater[areaID].UpdateTimer.Interval = _simulatorConfig.EmptyGameStateUpdateIntervalMS;
        }

        /// <summary>
        /// Stops simulation and removes all references to the GameState with the corresponding areaID
        /// </summary>
        /// <param name="areaID"></param>
        void StopSimulatingArea(int areaID)
        {
            if (_gameStates.ContainsKey(areaID))
            {
                _gameStates[areaID].Dispose();
                _areaIDToUpdater[areaID].UpdateTimer.Stop();
                _areaIDToUpdater.Remove(areaID);
                _gameStates.Remove(areaID);

            }
        }


        /// <summary>
        /// Sends a message over redis 
        /// </summary>
        public void ConnectToServer()
        {
            _redisServer.Subscribe(ChannelTypes.ServerToSimulator_Network, _simulatorID, HandleSimulatorNetworkMessage);

            var request = new MessageSimulatorConnectionRequest(){SimulatorID = _simulatorID};
            
            _redisServer.PublishObject(
                ChannelTypes.SimulatorToServer_Network,
                _simulatorID,
                new NetworkMessageContainer(request, MessageTypes.Redis_SimulatorConnectionRequest)
            );
        }

        void HandleSimulatorNetworkMessage(object sender, NetworkMessageContainer message)
        {
            if (message.MessageType == MessageTypes.Redis_SimulatorConnectionResponse)
            {
                _connectedToServer = true;
                ConsoleManager.WriteLine("Connected to server.", ConsoleMessageType.NetworkMessage);
            }

        }
    }
}
