//Client version 16.8
#define ADMIN

using System;
using Freecon.Client.Core;
using Freecon.Client.Config;
using Freecon.Core.Utils;
using System.Collections.Generic;
using System.Threading;
using Core.Logging;
using Freecon.Core.Configs;
using Freecon.Models.TypeEnums;
using RedisWrapper;
using Freecon.Server.Configs;
using Server.Managers;
using Simulator;

namespace Freecon.Simulator
{
    public class Simulator_Main
    {
        private ClientEnvironmentType _environment;
        private static SimulatorConfig _simulatorConfig;
        private static PhysicsConfig _physicsConfig;

        private static float _lastUpdateTimeStamp;
        private static MainManager _mainManager;

       
        //First argument is simulatorID (int), all subsequent arguments follow the format in _parseSimulationDatum
        public static void Main(string[] simulationData)
        {
            try
            {
                ConsoleManager.WriteLine("Starting simulator instance...", ConsoleMessageType.Debug);

                Logger.Initialize();

                TimeKeeper.Initialize();
                _simulatorConfig = new SimulatorConfig();
                _physicsConfig = new PhysicsConfig();
                RedisServer redisServer = new RedisServer(Logger.LogRedisError, Logger.LogRedisInfo, new RedisConfig().Address);
                RedisNetworkingService redisNetworkingService = new RedisNetworkingService(_simulatorConfig,
                    redisServer);
                int simulatorID = int.Parse(simulationData[0]);

                List<string> dataArray = new List<string>();
                for (int i = 1; i < simulationData.Length; i++)
                    dataArray.Add(simulationData[i]);


                _mainManager = new MainManager(simulatorID, redisNetworkingService,
                    new SimulatorGameTimeService(), _simulatorConfig, _physicsConfig);
                
                //IDs are parsed, then each ID is used to subscribe to a redis channel, which the server uses to communicate data about each system
                foreach (string s in dataArray)
                {
                    if (s == "")
                        continue;

                    var data = _parseSimulationDatum(s);
                    _mainManager.StartSimulatingArea(data.Item2, data.Item1);
                }


                _mainManager.ConnectToServer();
                
                Run();                
            }

            catch (Exception e)
            {
                ConsoleManager.WriteLine("Exception in simulator", ConsoleMessageType.Error);
                ConsoleManager.WriteLine(e.ToString(), ConsoleMessageType.Error);
                ConsoleManager.WriteLine(e.Message, ConsoleMessageType.Error);
                Run();
            }

        }

        static void Run()
        {
            while (true)
            {
                //TODO: move to time with autoreset disabled
                
                if (TimeKeeper.MsSinceInitialization - _lastUpdateTimeStamp >=
                    _simulatorConfig.MainManagerUpdateIntervalMS)
                {
                    _lastUpdateTimeStamp = TimeKeeper.MsSinceInitialization;

                    _mainManager.Update(null, null);

                    if (TimeKeeper.MsSinceInitialization - _lastUpdateTimeStamp >
                        _simulatorConfig.MainManagerUpdateIntervalMS)
                    {
                        ConsoleManager.WriteLine("WARNING: TIMED UPDATES LATE IN SIMULATOR",
                            ConsoleMessageType.Warning);
                    }
                }                
                Thread.Sleep(1);
            }
        }

        static Tuple<int, AreaTypes> _parseSimulationDatum(string datum)
        {

            //Data for each area to simulate is passed with the format [AreaType.ToString()]_[Id]
            string[] split = datum.Split('_');
            string areaTypeString = split[0];
            int areaID = int.Parse(split[1]);

            string[] allAreaTypes = Enum.GetNames(typeof(AreaTypes));
            Array allAreaTypesValues = Enum.GetValues(typeof(AreaTypes));

            var areaType = AreaTypes.Any;

            for(int i = 0; i < allAreaTypes.Length; i++)
            {
                if(areaTypeString == allAreaTypes[i])
                {
                    areaType = (AreaTypes)allAreaTypesValues.GetValue(i);
                    break;
                }

            }
            
            if(areaType == AreaTypes.Any)
            {
                throw new InvalidCastException("Unable to parse simulation data. AreaType not found.");
                
            }

            return new Tuple<int, AreaTypes>(areaID, areaType);


        }

        protected void Initialize()
        {
            ConsoleMover.SetConsolePosition(675, 340);

            Console.SetWindowSize(80, 50);
            Console.SetWindowPosition(0, 0);

        }
        

    }
}