using Core.Models.Enums;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.DB;
using Freecon.Core.Networking.Models.ServerToServer;
using Freecon.Server.Configs;
using Freecon.Server.Configs.Configs;
using RedisWrapper;
using Server.Database;
using Server.Managers;
using System;
using System.Collections.Generic;
using System.Timers;
using Freecon.Core.Utils;
using Freecon.Server.Core.Interfaces;
using Freecon.Server.Core.Services;
using Server.Models.Interfaces;

namespace MasterServer
{
    /// <summary>
    /// This class serves two functions: if no master servers exist on startup, it promotes this slave instance to a master server and manages it.
    /// If a master server already exists, this class serves as the interface between this slave and the master server.
    /// The fact that this class handles both the "slave" and possibly a master server might get confusing. Might break this apart soon.
    /// </summary>
    public class MasterServerManager:INetworkIDSupplier, ISlaveIDProvider
    {
        /// <summary>
        /// If true, this instance holds the current MasterServer. If false, true MasterServer is on a different process and/or machine 
        /// </summary>
        public bool IsMasterServer { get; set; }

        public MasterServerConfig MasterServerConfig { get; set; }
        public GalacticProperties GalacticProperties { get; set; }
        
        int? _id;
        /// <summary>
        /// Don't try to change this once it's been set. Server must be restarted to reset the Id, otherwise things will break.
        /// </summary>
        public int? SlaveID
        {
            get { return _id; }
            set
            {
                if (_id != null)
                {
                    throw new InvalidOperationException("Can't reset the SlaveID without restarting the server.");

                }
                else
                {
                    _id = value;
                }
            }
        }
        
        float _lastPingTime;

        RedisServer _redisServer { get; set; }

        MasterServer _masterServer;

        IDatabaseManager _databaseManager;
        IDbIdIoService _dbIdIoService;
        private readonly SlaveServerConfigService _slaveServerConfigService;

        bool _awaitingConnectionResponse;
        bool _connected;

        Timer _pingTimer;

        public MasterServerManager(MasterServerConfig c, GalacticProperties gp, IDatabaseManager dbm, IDbIdIoService dbIdIoService, RedisServer redisServer, SlaveServerConfigService slaveServerConfigService)
        {
            _redisServer = redisServer;
            _databaseManager = dbm;
            _dbIdIoService = dbIdIoService;
            _slaveServerConfigService = slaveServerConfigService;
            MasterServerConfig = c;
            GalacticProperties = gp;

            SlaveID = _slaveServerConfigService.CurrentServiceId;

            if (!_checkForMasterServer())
            {
                List<PSystemModel> allSystems = new List<PSystemModel>(_databaseManager.GetAllSystemsAsync().Result);
                _promoteToMasterServer(MasterServerConfig, GalacticProperties, allSystems, _databaseManager, _dbIdIoService, _redisServer);
            }
            else
            {
#if DEBUG
                ConsoleManager.WriteLine("Existing master server detected. Initializating as slave only.", ConsoleMessageType.Debug);
#endif
            }

            if (!IsMasterServer)
            {
                ConsoleManager.WriteLine("Initializing as slave only.", ConsoleMessageType.Notification);//Leaving this here to remember to log it later, might be useful
            }

            _connectToServer();

            redisServer.Subscribe(MessageTypes.Redis_SlaveConnectionResponse, _handleSlaveConnectionResponse);
            

            _pingTimer = new Timer(c.SlaveHeartbeatPeriodMS);
            _pingTimer.Elapsed += _updateSlaveHeartbeat;
            _pingTimer.Start();

        }

        public void Update(object sender, ElapsedEventArgs e)
        {
            if (_masterServer != null)
                _masterServer.UpdateAsync(TimeKeeper.MsSinceInitialization);
            else
            {
                if(!_checkForMasterServer())
                {
                    List<PSystemModel> allSystems = new List<PSystemModel>(_databaseManager.GetAllSystemsAsync().Result);
                    _promoteToMasterServer(MasterServerConfig, GalacticProperties, allSystems, _databaseManager, _dbIdIoService, _redisServer);
                }
            }

            var t = sender as System.Timers.Timer;
            if (t != null)
                t.Start();
                    
        }

        void _updateSlaveHeartbeat(object sender, ElapsedEventArgs e)
        {
            _redisServer.SetRawValue(RedisDBKeyTypes.SlaveHeartbeat, SlaveID.ToString(), "", TimeSpan.FromMilliseconds(MasterServerConfig.SlaveHeartbeatPeriodMS + MasterServerConfig.SlaveHeartbeatExpiryBuffer));
        }

       

        /// <summary>
        /// Returns true if another MasterServer is (presumed) online, false otherwise.
        /// Internally, an online MasterServer periodically posts a volatile timestamp to the redis DB. If no timestamp is found, the server is likely offline.
        /// </summary>
        /// <returns></returns>
        bool _checkForMasterServer()
        {
            var val =_redisServer.GetValue<MasterServerTimestamp>(RedisDBKeyTypes.MasterServerTimestamp);

            // Return if master server is online.
            return val != null;
        }
        
        void _handleSlaveConnectionResponse(object sender, NetworkMessageContainer messageData)
        {
            if (!_awaitingConnectionResponse)
            {
                return;
            }

            var data = messageData.MessageData as MessageSlaveConnectionResponse;

            if (data.SlaveID != SlaveID.Value)
            {
                return;
            }

            if (data.IsSuccessful)
            {
                _awaitingConnectionResponse = false;
                _connected = true;
                _redisServer.Subscribe(ChannelTypes.MasterSlave, SlaveID.Value, _handleSlaveDisconnectionDetected);
                return;
            }

            // Attempt connection again
            _connectToServer();
        }

        void _handleSlaveDisconnectionDetected(object sender, NetworkMessageContainer messageData)
        {
            //TODO: If we're nearing production, implement a graceful server shutdown, this unlikely message is received when the master server thinks the slave has disconnected.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates an ID and publishes a connection request to redis
        /// </summary>
        void _connectToServer()
        {
            _awaitingConnectionResponse = true;
            NetworkMessageContainer msg = new NetworkMessageContainer();
            msg.MessageData = new MessageSlaveConnectionRequest() { SlaveID = SlaveID.Value };
            msg.MessageType = MessageTypes.Redis_SlaveConnectionRequest;
            _redisServer.PublishObject(MessageTypes.Redis_SlaveConnectionRequest, msg);

        }

        /// <summary>
        /// Sets IsMasterServer=true if succesful
        /// </summary>
        /// <param name="c"></param>
        /// <param name="gp"></param>
        /// <param name="allSystemModels"></param>
        /// <param name="dbm"></param>
        /// <param name="redisServer"></param>
        void _promoteToMasterServer(MasterServerConfig c, GalacticProperties gp, IEnumerable<PSystemModel> allSystemModels, IDatabaseManager dbm, IDbIdIoService dbIdIoService, RedisServer redisServer)
        {

            #if DEBUG
            ConsoleManager.WriteLine("Promoting to master server...", ConsoleMessageType.Notification);  
            #endif

            
            string masterServerID = Rand.Random.Next(-int.MaxValue, int.MaxValue).ToString();
            

            bool setSuccessful = _redisServer.SetValue(
                RedisDBKeyTypes.MasterServerTimestamp,
                new MasterServerTimestamp() { MasterServerID = masterServerID },
                new TimeSpan(0, 0, 0, 0, c.InitializationTimestampTimeoutMS),
                SetWhen.NotExists
            );

            // Check if this instance was the first to set a timestamp, so we're clear to initialize a master server
            if (!setSuccessful)
            {
                ConsoleManager.WriteLine("Slave promotion failed, master server already exists, although master server check passed. Race Condition?", ConsoleMessageType.Error);
                // Another server exists already.
                return;
            }
            
            // Clear to initialize a master server.
            _masterServer = new MasterServer(c, gp, allSystemModels, dbm, dbIdIoService, redisServer);
            
            _masterServer.Id = int.Parse(masterServerID);   
                
            IsMasterServer = true;

#if DEBUG
            ConsoleManager.WriteLine("Master Server spawn Successful", ConsoleMessageType.Notification);
#endif
        }
        
        public void RequestFreeIDs(int numToRequest, IDTypes IdType)
        {
            //Go through redis, regardless of whether this instance is the master server
            var request = new MessageIDRequest() { IDType = IdType, RequestingServerID = SlaveID.Value, NumIDsRequested = numToRequest };
            var msg = new NetworkMessageContainer();
            msg.MessageType = MessageTypes.Redis_IDRequest;
            msg.MessageData = request;
            _redisServer.PublishObject(MessageTypes.Redis_IDRequest, msg);            
        }



    }
}
