using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Freecon.Server.Configs.Configs;
using Server.Managers;
using Server.Database;
using Freecon.Server.Configs;
using Server.GlobalIDManagers;
using Core.Models.Enums;
using RedisWrapper;
using Freecon.Core.Networking.Models.ServerToServer;
using Freecon.Core.Networking.Models.DB;
using System.Threading.Tasks;
using Core;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models;
using Freecon.Core.Utils;
using Freecon.Core.Networking.Messages;
using Freecon.Server.Core.Interfaces;

namespace MasterServer
{

    public class MasterServer
    {
        protected Dictionary<int, PSystemModel> _allSystemModels;

        public IReadOnlyDictionary<IDTypes, GlobalIDManager> GlobalIDManagers;

        protected float _initTimeMs;

        protected object _rebalanceLock = new object();

        public bool PauseRebalancing = false;//Pauses rebalancing, good for starting up multiple clients and avoiding data races during large DB reads, where a client receives instructions to stop handling IDs before it is finished reading all of them

        protected IDictionary<int, SlaveServer> _systemIDToSlaveServer;//Keeps track of which systems are being handled by which slaves

        protected Dictionary<int, SlaveServer> _slaveServers = new Dictionary<int, SlaveServer>();

        private IDatabaseManager _databaseManager;

        public int Id;

        protected RedisServer _redisServer;

        MasterServerConfig _config;

        protected float _lastTimeStamp = 0;

        bool _pendingRebalance;
        float _rebalanceWaitStartTime;

        //debug
        HashSet<int> _disconnectedSlaveIDs = new HashSet<int>();
        

        public MasterServer(MasterServerConfig c, GalacticProperties gp, IEnumerable<PSystemModel> allSystemModels, IDatabaseManager dbm, IDbIdIoService dbIdIoService, RedisServer redisServer)
        {
            _config = c;

            Dictionary<IDTypes, GlobalIDManager> globalIDManagers = new Dictionary<IDTypes, GlobalIDManager>();
            globalIDManagers.Add(IDTypes.GalaxyID, new GlobalGalaxyIDManager(dbIdIoService, gp));
            globalIDManagers.Add(IDTypes.TeamID, new GlobalTeamIDManager(dbIdIoService, gp));
            globalIDManagers.Add(IDTypes.TransactionID, new GlobalTransactionIDManager(dbIdIoService, gp));
            globalIDManagers.Add(IDTypes.AccountID, new GlobalAccountIDManager(dbIdIoService, gp));

            GlobalIDManagers = globalIDManagers;

            _redisServer = redisServer;
            _databaseManager = dbm;

            _systemIDToSlaveServer = new ConcurrentDictionary<int, SlaveServer>();  

            AccountManager_MasterServer acm = new AccountManager_MasterServer(new LocalIDManager_MS((GlobalAccountIDManager)globalIDManagers[IDTypes.AccountID], IDTypes.AccountID), dbm, true);

            _allSystemModels = new Dictionary<int, PSystemModel>();
            foreach(var sm in allSystemModels)
            {
                if(!_allSystemModels.ContainsKey(sm.Id))
                {
                    _allSystemModels.Add(sm.Id, sm);
                } 
            }

            _redisServer.Subscribe(MessageTypes.Redis_SlaveConnectionRequest, _slaveConnected);
            _redisServer.Subscribe(MessageTypes.Redis_IDRequest, _handleIDRequest);

            _initTimeMs = TimeKeeper.MsSinceInitialization;//If this class gets an initializer or a reset method, this should be moved there.
        }

        public async Task UpdateAsync(float currentTimeMS)
        {
            

            if(currentTimeMS - _lastTimeStamp > _config.TimestampPeriodMS)
            {
                //Check to make sure there aren't two master servers
                var data = await _redisServer.GetValueAsync<MasterServerTimestamp>(RedisDBKeyTypes.MasterServerTimestamp);

                if (data != null)
                {                   
                    
                    int receivedID = int.Parse(data.MasterServerID);


                    if (receivedID != Id)
                    {
                        throw new InvalidOperationException("Error: the ID associated with the current Master Server timestamp does not match this master server's Id. Are there two master servers running?");
                    }



                }
                else
                {
                    ConsoleManager.WriteLine("MasterServerTimestamp was null in redis db. Possible cause: MasterServer update was late.", ConsoleMessageType.Warning);
                    //If data is null, I'm assuming the master server was late in updating (which shouldn't happen in production, but was happening occasionally in debug.) Log just in case.
                }
                
                var msg = new MasterServerTimestamp() { MasterServerID = Id.ToString() };
                
                await _redisServer.SetValueAsync(RedisDBKeyTypes.MasterServerTimestamp, msg, new TimeSpan(0, 0, 0, 0, _config.TimestampTimeoutMS));

                _lastTimeStamp = currentTimeMS;
            }

            HashSet<int> slaveIDsToRemove = new HashSet<int>();

            //Check for disconnected slaves
            foreach(var s in _slaveServers)
            {
                var heartbeat = await _redisServer.GetRawValueAsync(RedisDBKeyTypes.SlaveHeartbeat, s.Value.ID.ToString());
                if (heartbeat == null && TimeKeeper.MsSinceInitialization - _initTimeMs > _config.InitializationTimestampTimeoutMS && TimeKeeper.MsSinceInitialization - s.Value.InitializationTime > _config.SlaveInitializationGracePeriod)
                {
                    _redisServer.ClearHashValue(RedisDBKeyTypes.SlaveIDHashSet, s.Value.ID);

                    

                    //ConsoleManager.WriteLine("Slave server timed out. Removing from slave list...", ConsoleMessageType.Warning);
                    slaveIDsToRemove.Add(s.Key);
                    _redisServer.PublishObjectAsync(ChannelTypes.MasterSlave, s.Value.ID, new NetworkMessageContainer(new MessageSlaveDisconnection(), MessageTypes.Redis_SlaveDisconnectionDetected));
                    _pendingRebalance = true;//TODO: implement more graceful rebalance on slave disconnect, as of this writing the server basically just resets entirely
                    _rebalanceWaitStartTime = TimeKeeper.MsSinceInitialization;
                }
                
            }

            slaveIDsToRemove.ForEach(key => { RemoveSlave(key); });



            if(_pendingRebalance && TimeKeeper.MsSinceInitialization - _rebalanceWaitStartTime > _config.RebalanceDelayMS)
            {
                _rebalanceServers();
            }

            
        }

        /// <summary>
        /// Checks the database for potentially incomplete handoffs which could result in lost players/ships
        /// </summary>
        /// <returns></returns>
        public async Task CheckHandoffCollections()
        {
            //If these collections aren't empty on server startup, a server crashed mid handoff and there could be ships/players which aren't stored in any area and might not be loaded.
            var ships = await _databaseManager.GetHandoffModelsAsync(ModelTypes.ShipModel);
            var players = await _databaseManager.GetHandoffModelsAsync(ModelTypes.PlayerModel);

            if (ships.Any() || players.Any())
            {
                ConsoleManager.WriteLine("ERROR ERROR ERROR ERROR ERROR WEEOOWEEOOWEEOOWEEOO SERVER LIKELY CRASHED MID HANDOFF, POSSIBLE MISSING SHIPS/PLAYERS! If this is debug, rerun dbfiller to fix state. If this is soon to be release, TODO: implement graceful recovery of \"lost\" ships/players.", ConsoleMessageType.Error);
            }

        }

        public int RemoveSlave(int slaveID)
        {
            var s = _slaveServers[slaveID];
            
            List<int> systemIDs = s.GetSystemIDs();

            foreach(int i in systemIDs)
                _systemIDToSlaveServer.Remove(i);

            
            _slaveServers.Remove(slaveID);

            ConsoleManager.WriteLine("WARNING: Rebalance not yet fully implemented. Ensure that slave servers properly reset (end all transactions, serialize all values, etc) when rebalance occurs.", ConsoleMessageType.Warning);

            _pendingRebalance = true;
            _rebalanceWaitStartTime = TimeKeeper.MsSinceInitialization;
            
            return s.ID;
        }           

        public int GetSlaveCount()
        {
            return _slaveServers.Count;

        }

        #region Message Handlers
        
        void _slaveConnected(object sender, NetworkMessageContainer messageData)
        {
            var data = messageData.MessageData as MessageSlaveConnectionRequest;

            var response = new MessageSlaveConnectionResponse();
            response.SlaveID = data.SlaveID;

            if (!_slaveServers.ContainsKey(data.SlaveID))
            {
                SlaveServer s = new SlaveServer(data.SlaveID);
                _slaveServers.Add(s.ID, s);
                response.IsSuccessful = true;
                _pendingRebalance = true;
                _rebalanceWaitStartTime = TimeKeeper.MsSinceInitialization;
            }
            else
            {
                response.IsSuccessful = false;
            }

            NetworkMessageContainer msg = new NetworkMessageContainer();
            msg.MessageData = response;
            msg.MessageType = MessageTypes.Redis_SlaveConnectionResponse;
            _redisServer.PublishObject(MessageTypes.Redis_SlaveConnectionResponse, msg);



        }

        void _handleIDRequest(object sender, NetworkMessageContainer messageData)
        {
            var data = messageData.MessageData as MessageIDRequest;


            if (GlobalIDManagers.ContainsKey(data.IDType))
            {
                var response = new MessageIDResponse();
                response.IDs = GlobalIDManagers[data.IDType].GetFreeIDs(data.NumIDsRequested);
                response.SlaveServerID = data.RequestingServerID;
                response.IDType = data.IDType;
                NetworkMessageContainer msg = new NetworkMessageContainer();
                msg.MessageData = response;
                msg.MessageType = MessageTypes.Redis_IDResponse;
                _redisServer.PublishObject(MessageTypes.Redis_IDResponse, msg);

            }

        }

        #endregion

        /// <summary>
        /// Reassigns areas to slave servers for simulation
        /// Used when servers connect
        /// </summary>
        private void _rebalanceServers()
        {
            lock (_rebalanceLock)
            {
                if (PauseRebalancing)
                    return;

                _systemIDToSlaveServer.Clear();
                foreach (var s in _slaveServers)
                    s.Value.ClearMySystems();


                

                if (_slaveServers.Count != 0)
                {
                    ConsoleManager.WriteLine("Rebalancing with " + _slaveServers.Count + " slaves.", ConsoleMessageType.Notification);

                    //this is temporary for now, and very inefficient
                    //Need to rewrite to minimize slave-slave handoffs of clients
                    int numSlaves = _slaveServers.Count;
                    List<int> systemIDs = new List<int>(_allSystemModels.Keys);

                    //Just makes the temporary redistribution easier
                    List<SlaveServer> slaveList = new List<SlaveServer>(_slaveServers.Count);

                    foreach (var s in _slaveServers)
                    {
                        slaveList.Add(s.Value);

                    }

                    //Distribute all systems among the slaves
                    for (int i = 0; i < systemIDs.Count; i++)
                    {
                        int currentSlaveNum = i % numSlaves;
                        int currentSlaveID = slaveList[currentSlaveNum].ID;

                        slaveList[currentSlaveNum].AddSystem(systemIDs[i]);
                        _systemIDToSlaveServer.Add(systemIDs[i], slaveList[currentSlaveNum]);


                    }

                    foreach (var s in _slaveServers)
                    {
                        var response = new MessageStartUpdatingSystems();
                        response.IDsToSimulate = s.Value.GetSystemIDs();
                        response.SlaveServerID = s.Key;
                        NetworkMessageContainer msg = new NetworkMessageContainer();
                        msg.MessageData = response;
                        msg.MessageType = MessageTypes.Redis_StartUpdatingSystems;
                        _redisServer.PublishObject(MessageTypes.Redis_StartUpdatingSystems, msg);

                    }

                }
                else
                {
                    SpawnSlave();
                }
            }
            _pendingRebalance = false;
        }

        void SpawnSlave()
        {
            throw new NotImplementedException("Nope. Slave spawn not implemented. If you got here, you probably need to restart the server. If you sometimes get this error on startup, increase MasterServerConfig.SlaveInitializationGracePeriod; If that doesn't work, fuck you you broke my server.");

        }
        

    }
}
