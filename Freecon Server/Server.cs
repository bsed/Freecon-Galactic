using Freecon.Core.Networking.Models;
using Freecon.Models.TypeEnums;
using Freecon.Server.Configs;
using Lidgren.Network;
using RedisWrapper;
using Server.Database;
using Server.Factories;
using Server.Managers;
using Server.Managers.Economy;
using Server.Managers.IncomingMessages;
using Server.Models;
using SRServer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Core.Models.Enums;
using MasterServer;
using System.Timers;
using Core.Logging;
using Freecon.Core.Configs;
using Freecon.Core.Utils;
using Freecon.Server.Configs.Configs;
using Freecon.Server.Core.Services;
using Server.Managers.ChatCommands;
using Server.Managers.OutgoingMessages;
using ChatManager = Server.Managers.ChatManager;
using CollisionManager = Server.Managers.CollisionManager;
using GalaxyManager = Server.Managers.GalaxyManager;
using Logger = Core.Logging.Logger;
using ProjectileManager = Server.Managers.ProjectileManager;
using ShipManager = Server.Managers.ShipManager;

namespace SRServer
{
    [System.Runtime.InteropServices.GuidAttribute("69D0AE8D-2F29-4E83-B335-7E1AD78B6EAC")]
    partial class Server
    {

        public static string VersionNumber = "SRServer Version 9001";


        ServerConfig ServerConfig = new ServerConfig();

        //Debug
        List<ShipTypes> _validTypes = new List<ShipTypes> { ShipTypes.Penguin, ShipTypes.Barge, ShipTypes.Reaper };
        int currentType = 1;

        PlayerManager _playerManager;
        LoginManager loginManager;
        AccountManager _accountManager;
        ConsoleManager _consoleManager;
        MasterServerManager _masterServerManager;

        ConnectionManager _connectionManager;
        WarpManager _warpManager;
        GalaxyManager _galaxyManager;
        ShipManager _shipManager;
        EconomyManager _economyManager;
        StructureManager _structureManager;

        ChatManager _chatManager;
        ClientUpdateManager _clientUpdateManager;
        CollisionManager _collisionManager;
        GalaxyRegistrationManager _registrationManager;
        ProjectileManager _projectileManager;
        RedisServer _redisServer;
        GlobalTeamManager _teamManager;
        IDatabaseManager _databaseManager;
        MessageManager _messageManager;
        KillManager _killManager;
        SimulatorManager _simulatorManager;

        CargoSynchronizer _cargoSynchronizer;

        List<ISynchronizer> _synchronizers;

        LocatorService _locatorService;

        LocalIDManager _teamIDManager;
        LocalIDManager _galaxyIDManager;
        LocalIDManager _transactionIDManager;
        LocalIDManager _accountIDManager;

        float _timeAtLastUpdate;

        bool _disableObjectUpdates = true;


        HashSet<MessageTypes> _typesToExcludeIgnore = new HashSet<MessageTypes>();
      //StreamWriter exceptionLogger = new StreamWriter("Exception_log.txt", true);


        static DBSyncer dbSyncer;
       
        MasterServerMessageHandler _msMessageHandler;

        List<System.Timers.Timer> UpdateTimers = new List<System.Timers.Timer>();

        System.Timers.Timer ConsoleRedrawTimer;

        // Update Statistic Variables
        private static int totalMessagesHandled = -2; // First two connection messages somehow aren't counted
        private static int numMsgHandled = 0; // Reset every loop
        private static Queue<float> msgAvgQue = new Queue<float>();
        private SlaveServerConfigService _slaveServerConfigService;

        /// <summary>
        /// Initializes the entire server
        /// </summary>
        private void Initialize()
        {
            for (int i = 0; i < 20; i++)
                msgAvgQue.Enqueue(0);

            var rand = new Random(3);

            // Logging Enabled
            Logger.Initialize();

            _synchronizers = new List<ISynchronizer>();


            //Load configs
            ConnectionManagerConfig connectionManagerConfig = new ConnectionManagerConfig(new CoreNetworkConfig());
            GalacticProperties galacticProperties = new GalacticProperties();

            _consoleManager = new ConsoleManager();

            _projectileManager = new ProjectileManager();

            ConsoleManager.WriteLine("Starting database...", ConsoleMessageType.Startup);
            _databaseManager = new MongoDatabaseManager();

            ShipStatManager.ReadShipsFromDBSList(_databaseManager.GetStatsFromDBAsync().Result);
            ConsoleManager.WriteLine("Ship types loaded.", ConsoleMessageType.Startup);

            RedisConfig rc = new RedisConfig();
            _redisServer = new RedisServer(LogRedisError, LogRedisInfo, rc.Address);

            var slaveId = SlaveServerConfigService.GetFreeSlaveID(_redisServer).Result;

            _slaveServerConfigService = new SlaveServerConfigService(_redisServer, slaveId);

            _masterServerManager = new MasterServerManager(new MasterServerConfig(), new GalacticProperties(), _databaseManager, _databaseManager, _redisServer, _slaveServerConfigService);

            _cargoSynchronizer = new CargoSynchronizer();
            _synchronizers.Add(_cargoSynchronizer);

            connectionManagerConfig.MyConfig.Port = ConnectionManager.GetFreePort(28002, 28010);
            _connectionManager = new ConnectionManager();
            _connectionManager.Initialize(connectionManagerConfig);

            //Poll to listen to Lidgren until it is ready
            //LidgrenMessagePoller_Init initializationPoller = new LidgrenMessagePoller_Init(_connectionManager.Server, this);
            //initializationPoller.Poll();

            _galaxyIDManager = new LocalIDManager(_masterServerManager, IDTypes.GalaxyID);
            _teamIDManager = new LocalIDManager(_masterServerManager, IDTypes.TeamID);
            _accountIDManager = new LocalIDManager(_masterServerManager, IDTypes.AccountID);
            _transactionIDManager = new LocalIDManager(_masterServerManager, IDTypes.TransactionID);

            _accountManager = new AccountManager(_accountIDManager, _databaseManager);

            _messageManager = new MessageManager(_connectionManager);

            _clientUpdateManager = new ClientUpdateManager(_playerManager);

            _playerManager = new PlayerManager(_databaseManager, _connectionManager, _redisServer, _galaxyIDManager, _clientUpdateManager);

            var chatCommands = new List<IChatCommand>()
            {
                new HelpCommand(),
                new ShoutCommand(_redisServer),
                new RadioCommand(),
                new TellCommand(_playerManager)
            };

            var asyncChatCommands = new List<IAsyncChatCommand>()
            {
                new AdminWarpCommand(_databaseManager, _redisServer, new Random())
            };

            _teamManager = new GlobalTeamManager(_teamIDManager, _connectionManager, _redisServer, _playerManager, _databaseManager);

            _galaxyManager = new GalaxyManager(galacticProperties.SolID, _teamManager);

            _chatManager = new ChatManager(chatCommands, asyncChatCommands, _playerManager, _messageManager, _redisServer);

            _warpManager = new WarpManager(_galaxyManager, _messageManager, _chatManager, _redisServer, _accountManager, _databaseManager);

            _shipManager = new ShipManager(_messageManager, _galaxyManager, _warpManager, _connectionManager, _databaseManager);

            _structureManager = new StructureManager(_databaseManager, _galaxyManager, _galaxyIDManager, _cargoSynchronizer);

            loginManager = new LoginManager(_accountManager, _playerManager, _connectionManager, _redisServer);

            // Todo: Convert everything over to ServerNetworkMessage to propogate full request context.
            _simulatorManager = new SimulatorManager(new SimulatorConfig(), _redisServer, (sender, container) => ProcessMessage(sender, new ServerNetworkMessage(container, null)));

            StructureStatManager.Initialize();

            ConsoleManager.WriteLine("Completed Initialization", ConsoleMessageType.Startup);

            _economyManager = new EconomyManager(_transactionIDManager, _playerManager, _galaxyManager, _cargoSynchronizer, _shipManager, _databaseManager, _masterServerManager);
            _killManager = new KillManager(_cargoSynchronizer, _playerManager, _galaxyManager, _messageManager, _connectionManager, _warpManager, _chatManager, _economyManager);
            _collisionManager = new CollisionManager(_galaxyManager, _messageManager, _killManager, _projectileManager);
            _registrationManager = new GalaxyRegistrationManager(_galaxyManager, _shipManager, _collisionManager, _galaxyIDManager, _playerManager, _accountManager, _cargoSynchronizer, _structureManager);
            _warpManager.SetRegistrationManager(_registrationManager);//Gross, I know.
            _locatorService = new LocatorService(_registrationManager, _playerManager, _galaxyManager, _shipManager, _accountManager, _teamManager, _teamManager, _messageManager, _structureManager, _masterServerManager);
            _msMessageHandler = new MasterServerMessageHandler((int)_masterServerManager.SlaveID, _redisServer, _connectionManager, _locatorService, _accountManager, _accountIDManager, _databaseManager, _galaxyManager, _galaxyIDManager, _playerManager, _shipManager, _registrationManager, _teamIDManager, _messageManager, _teamManager, _warpManager, _transactionIDManager, ProcessRoutedMessage);

            StructureFactory.Initialize(_galaxyIDManager, _registrationManager);
            ColonyFactory.Initialize(_galaxyIDManager, _registrationManager);

            dbSyncer = new DBSyncer(_databaseManager, _galaxyManager, _shipManager, _playerManager, _accountManager, _structureManager);

#if DEBUG
            _typesToExcludeIgnore.Add(MessageTypes.PositionUpdateData);
            _typesToExcludeIgnore.Add(MessageTypes.ShipFireRequest);
            _typesToExcludeIgnore.Add(MessageTypes.ProjectileCollisionReport);
            _typesToExcludeIgnore.Add(MessageTypes.StructureFireRequest);
            _typesToExcludeIgnore.Add(MessageTypes.ObjectPickupRequest);
#endif

        }

        public void Start()
        {
            Initialize();

            _disableObjectUpdates = true;//Don't forget this, or you'll end up with null errors

            System.Timers.Timer objectUpdater = GetTimer(ServerConfig.ObjectUpdatePeriod, _updateManagers);
            System.Timers.Timer structureUpdater = GetTimer(ServerConfig.StructureUpdatePeriod, _updateStructures);
            System.Timers.Timer syncTimer = GetTimer(ServerConfig.DBSyncPeriod, dbSyncer.SyncAllToDB);
            System.Timers.Timer economyManagerTimer = GetTimer(ServerConfig.TradeSynchronizerUpdatePeriod, _economyManager.Update); ;
            System.Timers.Timer masterServerTimer = GetTimer(ServerConfig.MasterServerManagerUpdatePeriod, _masterServerManager.Update); ;
            masterServerTimer.Start();//Must start this to receive systems to simulate
            
            UpdateTimers.Add(objectUpdater);
            UpdateTimers.Add(structureUpdater);
            UpdateTimers.Add(syncTimer);
            UpdateTimers.Add(masterServerTimer);
            UpdateTimers.Add(economyManagerTimer);

            LidgrenMessagePollerThreaded mp = new LidgrenMessagePollerThreaded(_connectionManager.Server, this);

            Thread MessagePoller = new Thread(mp.Poll);
            MessagePoller.Start();
            
            while (!_msMessageHandler.PSystemsLoaded)
            { 
                //Spinwait while systems are loading
                Thread.Sleep(1);
            }

            _simulatorManager.Initialize(_galaxyManager.Systems);

            TimeKeeper.Initialize();
            
            _disableObjectUpdates = false;

            foreach (var t in UpdateTimers)
                t.Start();
            
            _cargoSynchronizer.Start(ServerConfig.CargoSynchronizerUpdatePeriod, ServerConfig.CargoSynchronizerNumThreads);
        }

        //Utility method
        System.Timers.Timer GetTimer(float updatePeriod, Action<object, ElapsedEventArgs> updateEvent)
        {
            var rt = new System.Timers.Timer(updatePeriod);
            rt.Elapsed += new ElapsedEventHandler(updateEvent);
            rt.AutoReset = false;
            return rt;
        }
                
        public void Stop()
        { 
            foreach (var t in UpdateTimers)
            {
                t.Stop();
            }

            UpdateTimers.Clear();

            foreach(var s in _synchronizers)
            {
                s.Stop();
            }
        }

        public void Restart()
        {
            Stop();
            _disableObjectUpdates = true;
            Start();
        }
        
        void LogRedisInfo(string info, string key, string data)
        {
#if DEBUG
            //ConsoleManager.WriteLine(string.Format("{0}, {1}, {2}", info, key, data), ConsoleMessageType.Debug);
#endif

            Logger.Log(Log_Type.INFO, LowercaseContractResolver.SerializeObject(new {info, key, data}));
        }

        void LogRedisError(Exception e, string key, string data)
        {
            var highLevelMessage = "Redis error.";

#if DEBUG
            ConsoleManager.WriteLine(e.ToString(), ConsoleMessageType.Error);
#endif

            Logger.Log(Log_Type.ERROR, LowercaseContractResolver.SerializeObject(new {key, data, e, highLevelMessage}));
        }
        
        void _updateStructures(object source, ElapsedEventArgs e)
        {
            _structureManager.Update(TimeKeeper.MsSinceInitialization);
            var t = source as System.Timers.Timer;
            if (t != null)
                t.Start();

        }

        void _updateManagers(object source, ElapsedEventArgs ar)
        {
            if (_disableObjectUpdates)
                return;

            try
            {
                    _timeAtLastUpdate = TimeKeeper.MsSinceInitialization;

                    if (TimeKeeper.MsSinceInitialization - _timeAtLastUpdate > ServerConfig.ObjectUpdatePeriod)
                    {
                        ConsoleManager.WriteLine("WARNING. TIMED UPDATES LATE BY " + (TimeKeeper.MsSinceInitialization - _timeAtLastUpdate - ServerConfig.ObjectUpdatePeriod) + " ms.");
                    }

                    _galaxyManager.UpdateAreas(TimeKeeper.MsSinceInitialization);
                    _clientUpdateManager.Update(); // Checks time of clients, used to detect speed hacking
                    _projectileManager.Update();
                    _collisionManager.Update(_registrationManager, _projectileManager);

                    _connectionManager.Update();
                    _shipManager.Update();

                    Logger.Flush();               


            }
            catch (Exception e)
            {
                ConsoleManager.WriteLine(e.ToString(), ConsoleMessageType.Error);
                ConsoleManager.WriteLine(e.Message, ConsoleMessageType.Error);
                //exceptionLogger.Write(e.ToString());
                //exceptionLogger.Write(e.Message);
                //exceptionLogger.Write("\n\n");
                //exceptionLogger.Flush();
            }

            var t = source as System.Timers.Timer;
            if (t != null)
                t.Start();

        }
        

        /// <summary>
        /// Runs in a continuous loop on its own thread, delegating message handling to ThreadPool as necessary
        /// </summary>
        private class LidgrenMessagePollerThreaded
        {
            NetPeer _server; // Reference to lidgren object representing this slave server
            Server _p;

            public LidgrenMessagePollerThreaded(NetPeer server, Server p)
            {
                _server = server;
                _p = p;
            }

            public void Poll()
            {
                NetIncomingMessage msg;
               
                //TODO: FIGURE OUT THE BEST WAY TO POLL FOR AND PROCESS MESSAGES.
                while (true)
                {
                    // Messages are recycled in the threads that handle them
                    while ((msg = _server.ReadMessage()) != null)
                    {
                        //ConsoleManager.WriteLine("Processing " + msg.GetHashCode());
                        //Task.Run(() => {_p.ProcessMessage(msg);});
                        ThreadPool.QueueUserWorkItem(_p.ProcessLidgrenMessage, msg);
                        //_p.ProcessLidgrenMessage(msg);

                        totalMessagesHandled++;
                    }

                    Thread.Sleep(1);
                }
            }
        }

     
     
    }
}
