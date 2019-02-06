using System;
using System.Collections.Generic;
using System.Linq;
using Server.Managers;
using System.Collections.Concurrent;
using Freecon.Core.Networking.Models;
using SRServer.Services;
using Server.Models.Interfaces;
using Freecon.Models.TypeEnums;
using Server.Interfaces;
using Server.Models.Structures;
using Server.Models.Space;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Utils;

namespace Server.Models
{
    public abstract class Area<ModelType> : IArea, IHasStructures
        where ModelType : AreaModel, new()
    {        
        protected object SHIPLOCK = new object();
        protected object PLAYERLOCK = new object();

        protected ModelType _model { get; set; }

        #region State Variables

        //[BsonId(IdGenerator = typeof(DBIDGenerator))]
        //public int BsonID;

        public int Id {get { return _model.Id; } set { _model.Id = value; }}

        public int IDToOrbit {get { return _model.IDToOrbit; } set { _model.IDToOrbit = value; }}

        public int AreaSize {get { return _model.AreaSize; } set { _model.AreaSize = value; }}
        public int CurrentTrip {get { return _model.CurrentTrip; } set { _model.CurrentTrip = value; }}
        public int Distance {get { return _model.Distance; } set { _model.Distance = value; }}
        public int MaxTrip {get { return _model.MaxTrip; } set { _model.MaxTrip = value; }}

        public int? ParentAreaID {get { return _model.ParentAreaID; } set { _model.ParentAreaID = value; }}

        public string AreaName { get { return _model.AreaName; } set { _model.AreaName = value; } }

        protected float _lastUpdateTime;
        #endregion

        #region Objects in the Area

        //Players
        protected HashSet<int> _onlinePlayerIDs = new HashSet<int>();
        protected Dictionary<int, Player> _onlinePlayerCache = new Dictionary<int, Player>();//A cache, to minimize calls to _playerLocator.GetPlayer
        protected bool _isPlayerCacheReady = false;//Just in case a dev forgets to initialize the cache
        public IReadOnlyCollection<int> OnlinePlayerIDs { get { return (IReadOnlyCollection<int>)_onlinePlayerIDs; } }
        public int NumOnlinePlayers { get { return _onlinePlayerIDs.Count; } }
        public int NumOnlineHumanPlayers { get { return _humanPlayerIDs.Count; } }
        
        //Keep track of each player type to optimize some messages
        protected HashSet<int> _NPCPlayerIDs = new HashSet<int>();
        protected HashSet<int> _humanPlayerIDs = new HashSet<int>();
        public int NumNPCs { get { return _NPCPlayerIDs.Count; } }


        public List<ResourcePool> ResourcePools { get { return _model.ResourcePools; } set { _model.ResourcePools = value; } }//Placed here in case we want areas other than planets with resources, e.g. asteroids in space

        public ConcurrentDictionary<int, ISimulatable> SimulatableObjects = new ConcurrentDictionary<int, ISimulatable>(); 

        //Ships
        /// <summary>
        /// Ships currently in the system
        /// </summary>
        protected Dictionary<int, IShip> _shipCache = new Dictionary<int, IShip>();//A cache, to minimize calls to _playerLocator.GetPlayer. If this proves problematic, we can probably remove it.
        protected bool _isShipCacheReady = false;//Just in case a dev forgets to initialize the cache
        public int NumShips { get { return _model.ShipIDs.Count(); } }
        public IReadOnlyCollection<int> ShipIDs { get { return(IReadOnlyCollection<int>)_model.ShipIDs;} }

        #endregion


        /// <summary>
        /// Gets or sets the security level. 255 is maximum security, weapons disabled
        /// </summary>
        /// <value>
        /// The security level.
        /// </value>
        public byte SecurityLevel { get { return _model.SecurityLevel; } set { _model.SecurityLevel = value; } }

        protected Dictionary<int, IStructure> _structures { get; set; }

        public AreaTypes AreaType{get{return _model.AreaType;}}
        
        public List<Warphole> Warpholes { get { return _model.Warpholes; } set { _model.Warpholes = value; } }
        public float PosX {get { return _model.PosX; } set { _model.PosX = value; } }
        public float PosY {get { return _model.PosY; } set { _model.PosY = value; } }

        protected IPlayerLocator _playerLocator;
        protected IAreaLocator _areaLocator;
        protected IShipLocator _shipLocator;

        protected Area() 
        {
            _model = new ModelType();
            _structures = new Dictionary<int, IStructure>();
            _model.FloatySpaceObjects = new Dictionary<int, IFloatyAreaObject>();
        }

        protected Area(ModelType a, LocatorService ls)
        {
            _model = a;
            _playerLocator = ls.PlayerLocator;
            _areaLocator = ls.AreaLocator;
            _shipLocator = ls.ShipLocator;
            _structures = new Dictionary<int, IStructure>();
            _lastUpdateTime = TimeKeeper.MsSinceInitialization;
            
        }

        protected Area(int ID, LocatorService ls)
        {
            _model = new ModelType();
            Id = ID;

            _playerLocator = ls.PlayerLocator;
            _areaLocator = ls.AreaLocator;
            _shipLocator = ls.ShipLocator;
            _structures = new Dictionary<int, IStructure>();
            _model.FloatySpaceObjects = new Dictionary<int, IFloatyAreaObject>();
            _lastUpdateTime = TimeKeeper.MsSinceInitialization;
        }

        public virtual void Update(float currentTime)
        {
            _lastUpdateTime = currentTime;
        }

        public virtual Dictionary<int, IShip> GetShips()
        {
            Dictionary<int, IShip> ships;

            if (_isShipCacheReady)
            {
                return new Dictionary<int, IShip>(_shipCache);
            }

            _shipCache.Clear();
            ships = new Dictionary<int, IShip>(_onlinePlayerIDs.Count);
            var sl = _shipLocator.GetShipsAsync(_model.ShipIDs).Result;

            foreach (var s in sl)
            {
                ships.Add(s.Id, s);
                _shipCache.Add(s.Id, s);
            }

            _isShipCacheReady = true;
            
            return ships;
        }

        public virtual List<int> GetShipIDs()
        {
            List<int> ids = new List<int>(_model.ShipIDs.Count);
            foreach (int? s in _model.ShipIDs)
                ids.Add((int)s);

            return ids;
        }

        public virtual void AddShip(IShip s, bool suspendNetworking = false)
        {
            if(s is NPCShip)
            {
                AddShip((NPCShip)s, suspendNetworking);//Unexpected! Overloaded call resolves to AddShip(IShip)
                return;
            }


            _model.ShipIDs.Add(s.Id);
            lock (SHIPLOCK)
            {
                if (!_shipCache.ContainsKey(s.Id))
                    _shipCache.Add(s.Id, s);//Somehow, this sometimes throws an exception. Ergo lock.
                
            }

            if (!suspendNetworking)
            {
                var messageData = new MessageReceiveNewShips();
                messageData.Ships.Add(s.GetNetworkData(true, true, true, true));
                BroadcastMessage(new NetworkMessageContainer(messageData, MessageTypes.ReceiveNewShips), s.PlayerID);
            }
        }

        public virtual void AddShip(NPCShip npc, bool suspendNetworking=false)
        {
            _model.ShipIDs.Add(npc.Id);
            lock (SHIPLOCK)
            {
                if (!_shipCache.ContainsKey(npc.Id))
                    _shipCache.Add(npc.Id, npc);//Somehow, this sometimes throws an exception. Ergo lock.
                
            }
            SimulatableObjects.TryAdd(npc.Id, npc);

            if (!suspendNetworking)
            {
                var messageData = new MessageReceiveNewShips();
                messageData.Ships.Add(npc.GetNetworkData(true, true, false, true));
                BroadcastMessage(new NetworkMessageContainer(messageData, MessageTypes.ReceiveNewShips), npc.PlayerID);
            }
        }

        public virtual void RemoveShip(IShip s)
        {
 
            _model.ShipIDs.Remove(s.Id);
            _shipCache.Remove(s.Id);

            MessageRemoveKillRevive msgData = new MessageRemoveKillRevive { ActionType = ActionType.Remove, ObjectType = RemovableObjectTypes.Ship };
            msgData.ObjectIDs.Add(s.Id);
            BroadcastMessage(new NetworkMessageContainer(msgData, MessageTypes.RemoveKillRevive));
        }

        public virtual void RemoveShip(NPCShip npc)
        {

            ISimulatable temp;
            _model.ShipIDs.Remove(npc.Id);
            _shipCache.Remove(npc.Id);

            SimulatableObjects.TryRemove(npc.Id, out temp);

            MessageRemoveKillRevive msgData = new MessageRemoveKillRevive { ActionType = ActionType.Remove, ObjectType = RemovableObjectTypes.Ship };
            msgData.ObjectIDs.Add(npc.Id);
            BroadcastMessage(new NetworkMessageContainer(msgData, MessageTypes.RemoveKillRevive));
        }

        public virtual void MoveShipHere(IShip s)
        {
            if (s.GetArea() != null)
                s.GetArea().RemoveShip(s);

            AddShip(s);
        }

        /// <summary>
        /// Changes IShip area and moves IShip to specified position
        /// </summary>
        /// <param name="s"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        public virtual void MoveShipHere(IShip s, float xPos, float yPos)
        {
            if (s.GetArea() != null)
                s.GetArea().RemoveShip(s);

            AddShip(s);

            s.PosX = xPos;
            s.PosY = yPos;
        }

        public virtual void MoveShipHere(NPCShip npc)
        {
            if (npc.GetArea() != null)
                npc.GetArea().RemoveShip(npc);

            AddShip(npc);
        }

        public virtual void SetEntryPosition(IHasPosition warpingObject, IArea oldArea)
        {
            
        }

        /// <summary>
        /// Moves the player, sets simulating player in old and new systems appropriately.
        /// </summary>
        /// <param name="p"></param>
        public virtual void MovePlayerHere(Player p, bool isWarping)
        {
            if (p.CurrentAreaID != null)
                p.GetArea().RemovePlayer(p); //Removes player from his current area
            AddPlayer(p, isWarping); //Adds player to this area
        }

        public virtual void AddPlayer(Player p, bool isWarping)
        {
            lock (PLAYERLOCK)
            {
                
                if (p.IsOnline) //Only move the player here if he is online
                {
                    if (p.PlayerType == PlayerTypes.Human)
                    {
                        if(_NPCPlayerIDs.Count > 0)
                        {
                            //Notify simulator of login
                            GetOnlinePlayers()[_NPCPlayerIDs.ElementAt(0)].SendMessage(new NetworkMessageContainer(new MessageEmptyMessage { Data = _onlinePlayerIDs.Count + 1 }, MessageTypes.Redis_NumOnlinePlayersChanged));
                        }
                    }
                    if (!_onlinePlayerIDs.Contains(p.Id))
                    {
                        _onlinePlayerIDs.Add(p.Id);
                    }
                    else
                    {
                        ConsoleManager.WriteLine("Warning: _onlinePlayers already contained id belonging to player " + p.Username, ConsoleMessageType.Error);
                    }
                    if (!_onlinePlayerCache.ContainsKey(p.Id))
                    {
                        _onlinePlayerCache.Add(p.Id, p);
                    }
                    else
                    {
                        ConsoleManager.WriteLine("Warning: _onlinePlayerCache already contained id belonging to player " + p.Username, ConsoleMessageType.Error);
                    }
                }

                if(p.PlayerType == PlayerTypes.Human)
                {
                    _humanPlayerIDs.Add(p.Id);
                }
                else if(p.PlayerType == PlayerTypes.NPC)
                {
                    _NPCPlayerIDs.Add(p.Id);
                }

            }

        }

        /// <summary>
        /// Removes the player from the onlinePlayers dictionary appropriately
        /// Should be called on player.CurrentArea when warping
        /// </summary>
        /// <param name="p"></param>
        public virtual void RemovePlayer(Player p)
        {
            lock (PLAYERLOCK)
            {
                if (_onlinePlayerIDs.Contains(p.Id))
                {
                    _NPCPlayerIDs.Remove(p.Id);
                    _humanPlayerIDs.Remove(p.Id);


                    _onlinePlayerIDs.Remove(p.Id);
                    _onlinePlayerCache.Remove(p.Id);
                }
            }
        }

        public virtual void SendEntryData(HumanPlayer p, bool warping, IShip playerShip)
        {
            ConsoleManager.WriteLine("Warning, sendEntryShipData not implemented area of type " + this.GetType().ToString());
        }

        /// <summary>
        /// Creates projectiles and notifies other clients of firing ship
        /// </summary>
        /// <param name="firingPlayer"></param>
        /// <param name="weaponSlot"></param>
        public virtual void ShipFired(IShip firingShip, float rotation, byte weaponSlot, List<int> projectileIDs, IProjectileManager pm, byte percentCharge = 0)
        {
            foreach(var id in projectileIDs)
                pm.CreateProjectile(firingShip, id);

            MessageObjectFired messageData = new MessageObjectFired{ FiringObjectID = firingShip.Id, ObjectType = FiringObjectTypes.Ship, PercentCharge = percentCharge, Rotation = rotation, WeaponSlot = weaponSlot,ProjectileIDs = projectileIDs };
            BroadcastMessage(new NetworkMessageContainer(messageData, MessageTypes.ObjectFired), firingShip.Id, firingShip.PilotType == PilotTypes.NPC);//If NPC, simulator doesn't need to get spammed with ship fired notifications
            
        }
        /// <summary>
        /// Checks if a structure exists and can fire, fires and returns true if possible, false otherwise
        /// </summary>
        /// <param name="firingStructure"></param>
        /// <returns></returns>
        public virtual bool TryFireStructure(int firingStructureID, float rotationOfIncoming, List<int> projectileIDs, IProjectileManager pm, byte pctCharge, byte weaponSlot)
        {
            if (!_structures.ContainsKey(firingStructureID))
            {
                return false;
            }

            IStructure tempStructure = _structures[firingStructureID];

            if (tempStructure.StructureType == StructureTypes.DefensiveMine)
            {
                var mine = tempStructure as DefensiveMine;

                // Check if the mine was already triggered.
                if (mine == null || mine.WasTriggered)
                {
                    return false;
                }

                // Mine is removed automatically on the client when fired, no message is sent
                mine.WasTriggered = true;
                _structures.Remove(firingStructureID);
            }

            if (!tempStructure.Weapon.CanFire(tempStructure))
            {
                return false;
            }

            // if tempStructure is not ICanFire, we fucked up somewhere...
            tempStructure.Weapon.Fire(tempStructure);
            StructureFired((ICanFire)tempStructure, rotationOfIncoming, projectileIDs, pm, pctCharge, weaponSlot);
            return true;
        }
               
        public virtual void StructureFired(ICanFire firingStructure, float rotation, List<int> projectileIDs, IProjectileManager pm, byte percentCharge, byte weaponSlot)
        {
            foreach (var id in projectileIDs)
            {
                pm.CreateProjectile(firingStructure, id);
            }
            
            var messageData = new MessageObjectFired{ObjectType = FiringObjectTypes.Structure,FiringObjectID = firingStructure.Id,PercentCharge = percentCharge,Rotation = rotation,WeaponSlot = weaponSlot,ProjectileIDs = projectileIDs};

            BroadcastMessage(new NetworkMessageContainer(messageData, MessageTypes.ObjectFired), null);
        }

        /// <summary>
        /// Adds the floaty objects and sends messages to players as necessary
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public virtual void AddFloatyAreaObjects(IEnumerable<IFloatyAreaObject> objects)
        {
            var objectsAdded = new List<IFloatyAreaObject>();

            foreach (var obj in objects)
            {
                if (_model.FloatySpaceObjects.ContainsKey(obj.Id))
                {
                    ConsoleManager.WriteLine("Error: area already contains floaty space object.", ConsoleMessageType.Error);
                    continue;
                }

                _model.FloatySpaceObjects.Add(obj.Id, obj);
                objectsAdded.Add(obj);
            }

            if (_onlinePlayerIDs.Count <= 0 || objectsAdded.Count <= 0)
            {
                return;
            }
            
            var data = new MessageReceiveFloatyAreaObjects();

            foreach (var obj in objectsAdded)
            {
                data.FloatyObjects.Add(obj.GetFloatyNetworkData());                    
            }

            BroadcastMessage(new NetworkMessageContainer(data, MessageTypes.ReceiveFloatyAreaObjects));
        }
        
        /// <summary>
        /// Gets the object with the corresponding ID, if it exists
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public virtual IFloatyAreaObject GetFloatyAreaObject(int objectID)
        {
            if (_model.FloatySpaceObjects.ContainsKey(objectID))
            {
                return _model.FloatySpaceObjects[objectID];
            }

            return null;
        }

        public virtual void RemoveFloatyAreaObjects(IEnumerable<int> objectIDs)
        {
            HashSet<int> idsRemoved = new HashSet<int>();

            foreach (var obj in objectIDs)
            {
                if (_model.FloatySpaceObjects.ContainsKey(obj))
                {
                    _model.FloatySpaceObjects.Remove(obj);
                    idsRemoved.Add(obj);
                }
            }

            if (_onlinePlayerIDs.Count > 0 && idsRemoved.Count > 0)
            {
                MessageRemoveKillRevive msgData = new MessageRemoveKillRevive{ ActionType = ActionType.Remove, ObjectType = RemovableObjectTypes.FloatyAreaObject, ObjectIDs = idsRemoved };
                BroadcastMessage(new NetworkMessageContainer(msgData, MessageTypes.RemoveKillRevive));
            }
            
        }
              
        public abstract bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage);
        
        public virtual void AddStructure(IStructure s)
        {
            _model.StructureIDs.Add(s.Id);

            if (!_structures.ContainsKey(s.Id))
            {
                _structures.Add(s.Id, s);
            }

            SimulatableObjects.TryAdd(s.Id, s);
        }

        /// <summary>
        /// Currently, sendRemovalMessage should be false if a structure is killed, as it is removed on kill
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="sendRemovalMessage"></param>
        public virtual void RemoveStructure(int ID, bool sendRemovalMessage)
        {
            var removedStructure = _structures[ID];
            if(removedStructure == null)
                return;

            ISimulatable temp;
            SimulatableObjects.TryRemove(ID, out temp);
            

            _structures.Remove(ID);
            _model.StructureIDs.Remove(ID);
            if (sendRemovalMessage)
            {
                var msgData = new MessageRemoveKillRevive();
                msgData.ActionType = ActionType.Remove;
                msgData.ObjectIDs.Add(ID);
                msgData.ObjectType = RemovableObjectTypes.Structure;
                BroadcastMessage(new NetworkMessageContainer(msgData, MessageTypes.RemoveKillRevive));
            }
        }

        public virtual void RemoveStructure(object sender, int ID)
        {
            RemoveStructure(ID, true);
        }

        public virtual AreaEntryData GetEntryData(int? playerShipID, bool sendCargo = false, bool writeShipStats = false)
        {
            AreaEntryData data = new AreaEntryData();
            data.Id = Id;
            data.AreaName = AreaName;
            data.AreaSize = AreaSize;
            data.SecurityLevel = SecurityLevel;

            foreach (Warphole w in Warpholes)
            {
                data.Warpholes.Add(new WarpholeData() { WarpIndex = w.warpIndex, XPos = w.PosX, YPos = w.PosY, DestinationAreaID = w.DestinationAreaID });
            }

            foreach (KeyValuePair<int, IStructure> kvp in _structures)
            {
                data.Structures.Add(kvp.Value.GetNetworkData());
            }
            
            foreach (var s in GetShips())
            {
                if (s.Value.Id != playerShipID)
                {
                    data.Ships.Add(s.Value.GetNetworkData(true, true, sendCargo, writeShipStats));
                }
            }
            
            foreach (var f in _model.FloatySpaceObjects)
            {
                data.FloatyAreaObjects.Add(new FloatyAreaObjectData() { FloatyType = f.Value.FloatyType, Id = f.Value.Id, XPos = f.Value.PosX, YPos = f.Value.PosY, Rotation = f.Value.Rotation });             
            }

            return data;
        }

        /// <summary>
        /// Places structure in the nearest valid position, checking for overlap with other objects, and snaps the stucture to a grid
        /// </summary>
        /// <param name="structureType"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns>True if succesful, false if unable to place the structure</returns>
        public virtual bool GetValidStructurePosition(StructureStats stats, ref float xPos, ref float yPos)
        {
            bool isValid = true;

            //Snap to grid first
            xPos = (float)Math.Round(xPos + .5f) - .5f;//Round to the nearest .5
            yPos = (float)Math.Round(yPos + .5f) - .5f;



            //Check for overlap with structures
            foreach (var s in _structures)
            {
                isValid = !s.Value.CheckOverlap(stats.StructureSizeX, stats.StructureSizeY, xPos, yPos);

#if DEBUG
                if (!isValid)
                    ConsoleManager.WriteLine("Overlapping with an existing structure; setting isValid to false.");
#endif
                if (!isValid)
                    break;

            }

            return isValid;
        }
        
        public Dictionary<int, Player> GetOnlinePlayers()
        {
            lock (PLAYERLOCK)
            {
                try
                {
                    Dictionary<int, Player> players;
                    if (_isPlayerCacheReady)
                        players = new Dictionary<int, Player>(_onlinePlayerCache);

                    else
                    {
                        players = new Dictionary<int, Player>(_onlinePlayerIDs.Count);
                        foreach (int? id in _onlinePlayerIDs)
                        {
                            Player p = _playerLocator.GetPlayerAsync(id).Result;
                            if (p != null)
                                players.Add(p.Id, p);

                        }
                    }

                    return players;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLine(e.Message);
                    ConsoleManager.WriteLine(e.StackTrace);
                    return new Dictionary<int, Player>(_onlinePlayerCache);
                }
            }
        }

        public IReadOnlyDictionary<int, IStructure> GetStructures()
        {
            return _structures;

        }

        public IStructure GetStructure(int structureID)
        {
            if (_structures.ContainsKey(structureID))
                return _structures[structureID];
            else
                return null;

        }

        public HashSet<int> GetStructureIDs()
        {
            return new HashSet<int>(_model.StructureIDs);
        }
        
        public bool IsAreaConnected(int areaID)
        {
            foreach(Warphole w in Warpholes)
            {
                if (w.DestinationAreaID == areaID)
                    return true;
            }
            return false;
        }

        public IArea GetParentArea()
        {
            return _areaLocator.GetArea(ParentAreaID);
        }

        public virtual IDBObject GetDBObject()
        {
            return _model.GetClone();
        }

        /// <summary>
        /// Sends the message to all online players in this area, as appropriate
        /// </summary>
        /// <param name="message"></param>
        /// <param name="playerIDToIgnore">Message won't be sent to this ID if not null</param>
        public virtual void BroadcastMessage(NetworkMessageContainer message, int? playerIDToIgnore = null, bool sendToHumanClientsOnly = false)
        {
            if (_onlinePlayerIDs.Count > 0)
            {
                var players = GetOnlinePlayers();//Refreshes the cache if necessary

                //Send message to all online human clients
                foreach (var kvp in players)
                {
                    if (kvp.Value.Id != playerIDToIgnore && !_NPCPlayerIDs.Contains(kvp.Key))
                        kvp.Value.SendMessage(message);
                }
                
            }

            //Send single message to simulator since all players in an area are handled by the same simulator, no need to spam
            if (!sendToHumanClientsOnly && _NPCPlayerIDs.Count > 0)
            {
                var players = GetOnlinePlayers();//Refreshes the cache if necessary
                players[_NPCPlayerIDs.First()].SendMessage(message);
            }

        }
    
    }
}