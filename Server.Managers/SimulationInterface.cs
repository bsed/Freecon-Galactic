using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Simulator;
using Freecon.Core.Networking.Objects;
using Freecon.Simulator;
using RedisWrapper;
using Server.Models;

namespace Server.Managers
{

    public class SimulatorInterface
    {
        /// <summary>
        /// The areas that this Simulator is handling
        /// </summary>
        public List<int> SimulatedAreaIDs { get { return _simulatedAreaIDs; } protected set { _simulatedAreaIDs = value; } }
        List<int> _simulatedAreaIDs;

        RedisServer _redisServer;

        private SimulatorConfig _config;

        public int SimulatorID { get; set; }

        public bool IsConnected { get; protected set; }//TODO: Add ping

        /// <summary>
        /// Remember to unsubscribe old handler if this is changed.
        /// </summary>
        private EventHandler<NetworkMessageContainer> _messageHandler;

        /// <summary>
        /// Must call .Spawn() to launch a simulator process
        /// </summary>
        /// <param name="redisServer"></param>
        public SimulatorInterface(int simulatorID, SimulatorConfig config, RedisServer redisServer)
        {
            SimulatorID = simulatorID;
            SimulatedAreaIDs = new List<int>();
            _redisServer = redisServer;
            _config = config;

            _redisServer.Subscribe(ChannelTypes.SimulatorToServer_Network, SimulatorID, _handleSimulatorMessage);
        }

        /// <summary>
        /// returns the areaIDs that the simulator is now handling (all simulated aread in systemsToSimulate, including planets, moons, etc)
        /// </summary>
        /// <param name="systemsToSimulate"></param>
        /// <param name="messageHandler">handler to process incoming game data messages</param>
        /// <returns></returns>
        public void Spawn(List<PSystem> systemsToSimulate)
        {           
            Process.Start(_config.SimulatorExePath, GetArgs(systemsToSimulate, out _simulatedAreaIDs));                       
        }


        //TEMPORARY DEBUG SCAFFOLDING
        //=====================================================
        public void DEBUGSpawnLocal(List<PSystem> systemsToSimulate)
        {
            Thread t = new Thread((a) => { Simulator_Main.Main(GetArgs(systemsToSimulate, out _simulatedAreaIDs).Split(' ')); });
            t.Start();
            //Task t = new Task(() => {
            //                            Simulator_Main.Main(GetArgs(systemsToSimulate, out _simulatedAreaIDs).Split(' '));
            //});
            //t.Start();
        }                
        //=====================================================
        //TEMPORARY DEBUG SCAFFOLDING



        /// <summary>
        /// returns the argument string to be passed to the simulator process, containing all necessary data to load and begin simulating areas
        /// </summary>
        /// <param name="systemsToSimulate"></param>
        /// <param name="simulatedAreaIDs">All of the areas (systems, moons, planets, etc) being simulated</param>
        /// <returns></returns>
        public string GetArgs(List<PSystem> systemsToSimulate, out List<int> simulatedAreaIDs)
        {
            string args = SimulatorID + " ";
            simulatedAreaIDs = new List<int>();

            //TODO: get rid of argument list, just refactor Simulator to automatically spawn a new gamestate when SystemEntryData or PlanetEntryData is received
            //Spawn process
            foreach (var system in systemsToSimulate)
            {
                
                //Planets
                var planetString = string.Join("", system.GetPlanets().Select(p => p.AreaType + "_" + p.Id + " "));
                simulatedAreaIDs.AddRange(system.GetPlanets().Select(area => area.Id));

                //Moons
                var moonString = string.Join("", system.GetMoons().Select(m => m.AreaType + "_" + m.Id + " "));
                simulatedAreaIDs.AddRange(system.GetMoons().Select(area => area.Id));

                //System
                var systemString = system.AreaType + "_" + system.Id + " ";
                simulatedAreaIDs.Add(system.Id);

                args += planetString + moonString + systemString;
            }
            return args;
        }

        public List<int> SendAreaData(List<PSystem> systemsToSimulate, EventHandler<NetworkMessageContainer> messageHandler)
        {
            var areaIDs = new List<int>();

            foreach (var system in systemsToSimulate)
            {
                foreach (var p in system.GetPlanets())
                {
                    var edp = p.GetEntryData(null, true, true);

                    var planetData = new SimulatorBoundMessage(edp, MessageTypes.PlanetLandApproval, p.Id);

                    SendMessage(planetData);
                    areaIDs.Add(p.Id);
                }

                foreach (var m in system.GetMoons())
                {
                    var edm = m.GetEntryData(null, true, true);

                    var moonData = new SimulatorBoundMessage(edm, MessageTypes.PlanetLandApproval, m.Id);

                    SendMessage(moonData);
                    areaIDs.Add(m.Id);
                }

                var eds = system.GetEntryData(null, true, true);

                var systemData = new SimulatorBoundMessage(eds, MessageTypes.StarSystemData, system.Id);

                SendMessage(systemData);
                areaIDs.Add(system.Id);
            }

            _messageHandler = messageHandler;
            foreach (var i in areaIDs)
            {
                _redisServer.Subscribe(ChannelTypes.SimulatorToServer_Data, i, messageHandler);
            }

            return areaIDs;

        }

        void _handleSimulatorMessage(object sender, NetworkMessageContainer message)
        {
            if (message.MessageType == MessageTypes.Redis_SimulatorConnectionRequest)
            {
                var data = message.MessageData as MessageSimulatorConnectionRequest;
                IsConnected = true;
                var response = new MessageSimulatorConnectionResponse(){SimulatorID = SimulatorID};
                _redisServer.PublishObject(ChannelTypes.ServerToSimulator_Network, SimulatorID, new NetworkMessageContainer(response, MessageTypes.Redis_SlaveConnectionResponse));
                ConsoleManager.WriteLine("Simulator spawn complete, ID " + data.SimulatorID, ConsoleMessageType.Startup);
            }

        }

        ~SimulatorInterface()
        {
            _redisServer.UnSubscribe(ChannelTypes.SimulatorToServer_Network, SimulatorID, _handleSimulatorMessage);
            foreach (var i in SimulatedAreaIDs)
            {
                _redisServer.UnSubscribe(ChannelTypes.SimulatorToServer_Data, i, _messageHandler);
            }
        }

        public void SendMessage(SimulatorBoundMessage message)
        {
            _redisServer.PublishObject(ChannelTypes.ServerToSimulator_Data, message.TargetAreaId, message);
        }

        public async Task StartSimulatingArea(IArea area)
        {
            throw new NotImplementedException();

        }
    }
}
