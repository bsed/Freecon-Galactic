using RedisWrapper;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Models.Enums;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Objects;
using Freecon.Core.Utils;

namespace Server.Managers
{
    public class SimulatorManager
    {
        RedisServer _redisServer;

        private SimulatorConfig _config;

        private Dictionary<int, SimulatorInterface> _areaIdToSimulatorInterface;

        /// <summary>
        /// Designed for a single global event handler (server.ProcessMessage), but can be tweaked later
        /// </summary>
        private EventHandler<NetworkMessageContainer> _messageHandler;

        /// <summary>
        /// redisMessageHandler is a single entry point for processing all inbound redis messages from simulators.
        /// </summary>
        /// <param name="simulatorConfig"></param>
        /// <param name="redisServer"></param>
        /// <param name="redisMessageHandler"></param>
        public SimulatorManager(SimulatorConfig simulatorConfig, RedisServer redisServer, EventHandler<NetworkMessageContainer> messageHandler)
        {
            _redisServer = redisServer;
            _config = simulatorConfig;
            _areaIdToSimulatorInterface = new Dictionary<int, SimulatorInterface>();
            _messageHandler = messageHandler;

        }


        /// <summary>
        /// To be called at server start. Spawns simulators and assigns all systems and contained areas for simulation.
        /// </summary>
        /// <param name="allSystems"></param>
        public void Initialize(ICollection<PSystem> allSystems)
        {
            int numAssigned = 0;

            List<PSystem> assignThese = new List<PSystem>();
            while (numAssigned < allSystems.Count)
            {
                assignThese.Add(allSystems.ElementAt(numAssigned));
                numAssigned++;
                if (assignThese.Count == _config.NumAreasPerSimulator || numAssigned == allSystems.Count)
                {
                    SpawnSimulator(assignThese, true);
                    assignThese.Clear();
                }
            }

        }


        /// <summary>
        /// Spawns a simulator process (currently on this machine) with the specified areasToSimulate
        /// </summary>
        /// <param name="areasToSimulate"></param>
        /// <returns></returns>
        public void SpawnSimulator(List<PSystem> areasToSimulate, bool DEBUGSPAWNLOCAL = false)//TODO: Remove DEBUGSPAWNLOCAL once debugging is complete
        {
            var newsim = AttemptSpawn(areasToSimulate, DEBUGSPAWNLOCAL);
            float waitStartTime = TimeKeeper.MsSinceInitialization;

            int numAttempts = 1;

            while (!newsim.IsConnected) //Spinwait until connected or timeout
            {
                Thread.Sleep(1);
                if (numAttempts == _config.MaxNumConnectionAttempts && TimeKeeper.MsSinceInitialization - waitStartTime > _config.ConnectionTimeoutMS)
                {

                    throw new Exception("Could not spawn simulator. Maximum number of connection attempts exceeded.");
                }
                else if (TimeKeeper.MsSinceInitialization - waitStartTime > _config.ConnectionTimeoutMS)
                {
                    newsim = AttemptSpawn(areasToSimulate, DEBUGSPAWNLOCAL);
                    waitStartTime = TimeKeeper.MsSinceInitialization;
                    numAttempts++;
                }

                
                
            }

            var ids = newsim.SendAreaData(areasToSimulate, _messageHandler);


            foreach (var i in ids)
            {
                _areaIdToSimulatorInterface.Add(i, newsim);
            }
            
        }

        SimulatorInterface AttemptSpawn(List<PSystem> areasToSimulate, bool DEBUGSPAWNLOCAL=false)//TODO:Remove DEBUGSPAWNLOCAL Once debugging is complete
        {
            
            int simulatorID = Rand.Random.Next(-int.MaxValue, int.MaxValue); //TODO: remove collision potential
            ConsoleManager.WriteLine("Attempting simulator spawn, ID " + simulatorID, ConsoleMessageType.Startup);

            var newsim = new SimulatorInterface(simulatorID, _config, _redisServer);

            if (!DEBUGSPAWNLOCAL)
            {
                newsim.Spawn(areasToSimulate);
            }

            else
            {
                newsim.DEBUGSpawnLocal(areasToSimulate);
            }


            return newsim;

        }


        public void SendMessageToSimulator(SimulatorBoundMessage message)
        {
            if (_areaIdToSimulatorInterface.ContainsKey(message.TargetAreaId))//Not all areas are simulated currently, e.g. colonies
            {
                _areaIdToSimulatorInterface[message.TargetAreaId].SendMessage(message);
            }
            else
            {
                ConsoleManager.WriteLine("Attempted to send simulator message to a non simulated area.", ConsoleMessageType.Error);
            }
        }



    }

    

    public class SimulatorConfig
    {
        public string SimulatorExePath;

        public int NumAreasPerSimulator;
        
        /// <summary>
        /// Make sure this is large enough to allow for connection!
        /// </summary>
        public float ConnectionTimeoutMS;

        public int MaxNumConnectionAttempts;

        public float PingInterval;

        public float PingTimeout;

        public SimulatorConfig()
        {
            //TODO:Switch to relative path before moving to remote machine launch
            //I'll come back to this someday
            
            var exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);//Straight copy paste. First line in 2018?
            string targetPath = "";
            exePath.Split(System.IO.Path.DirectorySeparatorChar).Take(4).ForEach(s => { targetPath += s + "/"; });


#if DEBUG
            //SimulatorExePath = @"C:\SRDevGit\freecon-galactic\SRServer\Simulator\bin\x86\Debug\Simulator.exe";
            SimulatorExePath = targetPath + "/Simulator/bin/x82/Debug/Simulator.exe";
#else
            SimulatorExePath = targetPath + "/Simulator/bin/x82/Release/Simulator.exe";
            SimulatorExePath = @"C:\SRDevGit\freecon-galactic\SRServer\Simulator\bin\x86\Release\Simulator.exe";
#endif
            NumAreasPerSimulator = 10;

            ConnectionTimeoutMS = 3000;

            MaxNumConnectionAttempts = 1;
        }

    }
}
