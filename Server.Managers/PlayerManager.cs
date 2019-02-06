using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Linq;
using System.Collections.Concurrent;
using Freecon.Core.Networking.Models;
using SRServer.Services;
using Server.Models;
using Server.Database;
using RedisWrapper;
using Server.Models.Interfaces;
using System.Threading.Tasks;
using Freecon.Core.Networking.ServerToServer;
using Freecon.Core.Interfaces;
using Server.Managers.OutgoingMessages;

namespace Server.Managers
{
    public class PlayerManager : ObjectManager<Player, PlayerModel>, IPlayerLocator
    {
        public int numOnline { get { return connectionToPlayer.Count; } }

        ConcurrentDictionary<string, Player> _usernameToPlayer;
        public ConcurrentDictionary<NetConnection, Player> connectionToPlayer;//TODO: replace or supplement with unique session ID

        private float lastTimeStamp = 0; //In milliseconds
        private float timeElapsed = 0; //In milliseconds
        
        ClientUpdateManager _clientUpdateManager;

        ConnectionManager _connectionManager;
        RedisServer _redisServer;
        LocalIDManager _playerIDManager;

        Server.Utilities.NameProvider _npcNameProvider = new Server.Utilities.NameProvider(@"C:\SRDevGit\freecon-galactic\NPC Names.txt");

        Random r = new Random(666);

        public PlayerManager(IDatabaseManager databaseManager, ConnectionManager cm, RedisServer rs, LocalIDManager idm, ClientUpdateManager cum):base(databaseManager)
        {
            _usernameToPlayer = new ConcurrentDictionary<string, Player>();
            connectionToPlayer = new ConcurrentDictionary<NetConnection, Player>();

            _clientUpdateManager = cum;
            _connectionManager = cm;
            _redisServer = rs;
            _playerIDManager = idm;

            _redisServer.Subscribe(MessageTypes.Redis_PlayerRemoveTeam, _teamRemove_Handler);
        }        

        /// <summary>
        /// Adds a new player using an existing account
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="username"></param>
        /// <param name="connection"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public Player CreateHumanPlayer(string username, Account account, LocatorService ls)
        {
            var p = new HumanPlayer(_playerIDManager.PopFreeID(), username, account, ls);
            
            //p.area = new Area();
            _objects.TryAdd(p.Id, p);

            _usernameToPlayer.TryAdd(username, p);
            if (!_usernameToPlayer.ContainsKey(username.ToLower()))
                // Any player that has Caps in their name is added twice
                _usernameToPlayer.TryAdd(username.ToLower(), p);

            p.CashOnHand = 1000000; // I am a kind god ;D
            return p;
        }

        public NPCPlayer CreateNPCPlayer(LocatorService ls)
        {
            string username = _npcNameProvider.GetRandomName(2, a=>{return a[0] + " " + a[1];});
            while (_usernameToPlayer.ContainsKey(username.ToLower()))
                username += _npcNameProvider.GetRandomName();


            var p = new NPCPlayer(_playerIDManager.PopFreeID(), username, ls);
            //p.area = new Area();
            _objects.TryAdd(p.Id, p);

            _usernameToPlayer.TryAdd(username, p);
            if (!_usernameToPlayer.ContainsKey(username.ToLower()))
                // Any player that has Caps in their name is added twice
                _usernameToPlayer.TryAdd(username.ToLower(), p);

            p.CashOnHand = 1000000; // I am a kind god ;D
            return p;
        }

        /// <summary>
        /// Registers a player with the manager. If the player already exists, it is replaced with the passed version (acts like an update, useful for refreshes from DB)
        /// </summary>
        /// <param name="p"></param>
        public override void RegisterObject(Player p)
        {
            base.RegisterObject(p);

            _usernameToPlayer.AddOrUpdate(p.Username, p, (k, v) => p);
            if (!_usernameToPlayer.ContainsKey(p.Username.ToLower()))
                // Any player that has Caps in their name is added twice, to keep people from reusing player names with different capitalizations
                _usernameToPlayer.AddOrUpdate(p.Username.ToLower(), p, (k,v)=>p);                             
        }        

        /// <summary>
        /// Same as calling RegisterObject
        /// </summary>
        /// <param name="p"></param>
        public void RegisterPlayer(Player p)
        {
            RegisterObject(p);
        }

        public void DeregisterPlayer(int ID)
        {
            DeregisterObject(ID);
        }

        /// <summary>
        /// Removes player with specified ID
        /// </summary>
        /// <param name="ID"></param>
        public override void DeregisterObject(int ID)
        {

            if (_objects.ContainsKey(ID))
            {
                Player p = _objects[ID];

                _usernameToPlayer.TryRemove(p.Username, out p);

                if (p.PlayerType == PlayerTypes.Human && p.MessageService != null)
                {
                    connectionToPlayer.TryRemove(((LidgrenOutgoingMessageService) p.MessageService).Connection, out p);
                }

                
            }

            base.DeregisterObject(ID);

        }
     
        /// <summary>
        /// If a heartbeat is received from a player, we set a datetime of the event.
        /// </summary>
        /// <param name="msg">incoming message</param>
        public void playerHeartbeat(NetIncomingMessage msg)
        {
            int playerValue;
            try
            {
                playerValue = msg.ReadByte();
            }
            catch
            {
                return;
            }
            var p = GetObjectAsync(playerValue).Result;
            p.lastHeartbeat = DateTime.Now.Ticks/10000L;
        }

        /// <summary>
        /// This function checks if a player has timed out or not.
        /// </summary>
        public void updatesHeartbeats()
        {
            var currentTime = DateTime.Now.Ticks/10000L;
            
            foreach(var player in _objects)
            {
                if (player.Value.IsOnline)
                {
                    var elapsedTime = currentTime - player.Value.lastHeartbeat;

                    if (elapsedTime > player.Value.disconnectTime) // Disconnect if true
                    {
                        ConsoleManager.WriteLine(
                                         string.Format("Disconnecting Player {0}", player.Value.Username));
                        player.Value.LogOut(_databaseManager);
                    }
                }
            }
        }

        public bool IsPlayerLocal(int playerId)
        {
            return _objects.ContainsKey(playerId);
        }

        protected override Player _instantiateObject(IDBObject pm, LocatorService ls)
        {
            PlayerModel model = pm as PlayerModel;
            if (model.PlayerType == PlayerTypes.Human)
            {
                return new HumanPlayer(model, ls);
            }
            else
            {
                var p = new NPCPlayer(model, ls);
                p.MessageService = new RedisOutgoingMessageService(_redisServer, p);
                return p;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="fetchFromDB">If true, ls cannot be null</param>
        /// <param name="ls">Cannot be null if fetchFromDB is true</param>
        /// <param name="persistFetch">If true, fetched instance is stored/updated in the manager's list of players</param>
        /// <returns></returns>
        public async Task<Player> GetPlayerAsync(int? ID, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true)
        {
            return await GetObjectAsync(ID, fetchFromDB, ls, persistFetch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="fetchFromDB">If true, ls cannot be null</param>
        /// <param name="ls">Cannot be null if fetchFromDB is true</param>
        /// <param name="persistFetch">If true, fetched instance is stored/updated in the manager's list of players</param>
        /// <returns></returns>
        public async Task<Player> GetPlayerAsync(string username, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true)
        {
            Player p = null;//Default value if s isn't found

            if (fetchFromDB)
            {
                if (ls == null)
                {
                    throw new Exception("Cannot fetch a player from the database without a reference to a LocatorService object.");
                }
                p = _instantiateObject(await _databaseManager.GetPlayerAsync(username), ls);

                if (persistFetch && p != null)
                {
                    RegisterObject(p);
                }
            }
            else
            {
                if (_usernameToPlayer.ContainsKey(username))
                    p = _usernameToPlayer[username];
            }

            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="fetchFromDB">If true, ls cannot be null</param>
        /// <param name="ls">Cannot be null if fetchFromDB is true</param>
        /// <param name="persistFetch">If true, fetched instance is stored/updated in the manager's list of players</param>
        /// <returns></returns>
        public async Task<ICollection<Player>> GetPlayersAsync(List<int> IDs, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true)
        {
            return await GetObjectsAsync(IDs, fetchFromDB, ls, persistFetch);
        }


        /// <summary>
        /// Finds the player with the corresponding Username
        /// Returns the player in the out Player p object
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Player GetPlayer(string username)
        {
            return _usernameToPlayer.ContainsKey(username) ? _usernameToPlayer[username] : null;
        }
        

        public Player GetPlayer(NetConnection connection)
        {
            Player p;

            if (connectionToPlayer.ContainsKey(connection))
                p = connectionToPlayer[connection];
            else
            {
                ConsoleManager.WriteLine("Failed to find player using connection");
                p = null;
            }

            return p;
        }

        public List<Player> GetOnlinePlayers()
        {
            return _objects.Select(p => p.Value).ToList();
        }

        public void LogOut(int playerID)
        {
            LogOut(_objects[playerID]);
        }

        public void LogOut(NetConnection playerConnection)
        {
            LogOut(connectionToPlayer[playerConnection]);
        }

        public void LogOut(Player p)
        {
            if (p.PlayerType != PlayerTypes.Human)
                return;
            
            p.LogOut(_databaseManager);

            Player temp;
            connectionToPlayer.TryRemove(((LidgrenOutgoingMessageService)p.MessageService).Connection, out temp);


            ConsoleManager.WriteLine("Player " + p.Username + " has been logged out.", ConsoleMessageType.Notification);
        }

        /// <summary>
        /// Writes all players to the db
        /// </summary>
        /// <param name="dbr"></param>
        public async Task SyncPlayers(IDBWriter dbr)
        {
            //TODO: Add lock
            await dbr.SaveAsyncBulk(_objects.Values.ToList());
        }

        void _teamRemove_Handler(object sender, NetworkMessageContainer messageData)
        {
            PlayerRemoveTeam t = messageData.MessageData as PlayerRemoveTeam;

            var areas = new HashSet<IArea>();
            foreach (int i in t.PlayerIDs)
            {
                if(_objects.ContainsKey(i))
                {             
                    _objects[i].RemoveTeam(t.TeamIDToRemove);
                    areas.Add(_objects[i].GetArea());
                }
            }

          
          
            //Send removeships message to areas in which this team exists
            foreach (var a in areas)
            {
                var data = new MessageAddRemoveShipsTeam{ AddOrRemove = false, TeamID = t.TeamIDToRemove };

                foreach (var s in a.GetShips())
                {
                    if (t.PlayerIDs.Contains(s.Value.Id))
                        data.IDs.Add(s.Value.Id);
                }

                var message = new NetworkMessageContainer(data, MessageTypes.AddRemoveShipsTeam);
                a.BroadcastMessage(message);

            }
        
        }

        



    }
}