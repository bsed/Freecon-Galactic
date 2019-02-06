using System;
using System.Collections.Generic;
using Freecon.Core.Interfaces;
using SRServer.Services;
using Server.Models.Interfaces;
using Server.Interfaces;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Objects;

namespace Server.Models
{
    public abstract class Player : ISerializable, ITeamable
    {
        protected PlayerModel _model;

        public bool CheatEngineOpen;
        public int Id { get { return _model.Id; } }

        public int? AccountID { get { return _model.AccountID; } set { _model.AccountID = value; } }

        public RonBurgundy NewsManager { get { return _model.NewsManager; } set { _model.NewsManager = value; } }

        public float CashInBank { get { return _model.CashInBank; } set { _model.CashInBank = value; } }
        public float CashOnHand { get { return _model.CashOnHand; } set { _model.CashOnHand = value; } }
        public TimeSpan ClientTime;
        //public NetConnection Connection;

        public int? CurrentAreaID { get { return _model.CurrentAreaID; } }

        public IOutgoingMessageService MessageService;

        public float disconnectTime = 6000;
                     //Maximum number of milliseconds between communications before player is disconnected

        public int? ActiveShipId { get { return _model.ActiveShipId; } }

        public int HackCount { get { return _model.HackCount; } set { _model.HackCount = value; } }
        public virtual bool IsOnline { get; set; }

        public long lastHeartbeat;
        public int PlayersKilled { get { return _model.PlayersKilled; } set { _model.PlayersKilled = value; } }
        public DateTime serverLastTime;

        protected IAreaLocator _areaLocator;
        protected IShipLocator _shipLocator;
        protected IAccountLocator _accountLocator;
        protected ITeamLocator _teamLocator;

        protected Dictionary<int, Team> _teamCache = new Dictionary<int, Team>();
        protected bool _isTeamCacheReady = false;

        /// <summary>
        /// Every player gets a unique default teamID on creation, for things like structures
        /// </summary>
        public int DefaultTeamID { get { return _model.DefaultTeamID; } }

        public bool IsTrading { get; set; }

        public HashSet<int> ColonizedPlanetIDs { get { return _model.ColonizedPlanetIDs; } set { _model.ColonizedPlanetIDs = value; } }

        public string Username { get { return _model.Username; } set { _model.Username = value; } }

        public bool IsHandedOff = false;//Set to true after a handoff to avoid logout on disconnection

        public PlayerTypes PlayerType { get { return _model.PlayerType; } }

        public Player() {

            _model = new PlayerModel();
            _model.PlayerType = PlayerTypes.Human;
        }

        public Player(PlayerModel p, LocatorService ls)
        {

            _areaLocator = ls.AreaLocator;
            _shipLocator = ls.ShipLocator;
            _accountLocator = ls.AccountLocator;
            _teamLocator = ls.TeamLocator;

            _model = p;
           
        }


        public Player(int playerID, string name, Account account, LocatorService ls)
        {
            _model = new PlayerModel();

            _model.Id = playerID;
            Username = name;

            IsOnline = false;
            CheatEngineOpen = false;
            HackCount = -1;
            
            _areaLocator = ls.AreaLocator;
            _shipLocator = ls.ShipLocator;
            _accountLocator = ls.AccountLocator;
            _teamLocator = ls.TeamLocator;

            _model.AccountID = account.Id;

            _model.DefaultTeamID = ls.TeamManager.CreateNewTeam(this);
            _model.TeamIDs.Add((int)_model.DefaultTeamID);
            _model.PlayerType = PlayerTypes.Human; 
            
        }
        /// <summary>
        /// Does extra stuff when player changes area
        /// MUST BE CALLED AFTER AREA IS CHANGED
        /// </summary>
        public void doWarpStuff()
        {
        }

        private void OnDisconnect()
        {
            var tempArea = _areaLocator.GetArea(_model.CurrentAreaID);  
            tempArea.RemovePlayer(this);
        }

        /// <summary>
        /// SHOULD ONLY BE CALLED BY a PlayerManager instance
        /// Logs the player out and pushes the player, account, and all ships to the DB
        /// </summary>
        public virtual void LogOut(IDBWriter dbWriter)
        {
            OnDisconnect();
            IsOnline = false;
            dbWriter.SaveAsync(this);
            dbWriter.SaveAsync(_shipLocator.GetShipAsync(_model.ActiveShipId).Result);
            //account.LastSystemID = CurrentArea.Id;
            dbWriter.SaveAsync(GetAccount());

            dbWriter.SaveAsync(_areaLocator.GetArea(_model.CurrentAreaID));//Required in case client warps, logs out, and server crashes before area is written to DB
                                                                           //If this proves excessive, we'll probably have to force all ships to a certain area or manually ensure that IShip areas and area.ship match on server restart, which we'll probably have to do anyway

        }

        public void SendMessage(MessagePackSerializableObject data,
            MessageTypes messageType,
            RoutingData routingData = null)
        {
            SendMessage(new NetworkMessageContainer(data, messageType, routingData));
        }

        public void SendMessage(NetworkMessageContainer msg)
        {
            MessageService?.SendMessage(msg);
        }
        
        public virtual void SetArea(IArea newArea)
        {
            if (newArea != null)
            {
                if (_model.PlayerType == PlayerTypes.Human && newArea.AreaType == AreaTypes.System)
                    GetAccount().LastSystemID = newArea.Id;

                _model.CurrentAreaID = newArea.Id;
            }
            else
                _model.CurrentAreaID = null;
            
            
            

        }

        /// <summary>
        /// Automatically warps player to activeShip location. Current architecture requires player to be located in same area as the active ship.
        /// </summary>
        /// <param name="s">nullable</param>
        /// <param name="wm"></param>
        /// <param name="suspendDbWrite"></param>
        /// <returns>Previous activeShipId </returns>
        public int? SetActiveShip(IShip s, IWarpManager wm, bool suspendDbWrite = false)
        {
            if (s.PlayerID != Id)
                throw new InvalidOperationException("Must call IShip.SetPlayer() before calling SetActiveShip()");

            int? oldId = _model.ActiveShipId;
            _model.ActiveShipId = s.Id;
            _model.OwnedShipIds.Add(s.Id);
            if (s.CurrentAreaId.HasValue)
                wm.ChangeArea((int)s.CurrentAreaId, s, false, !suspendDbWrite);
            return oldId;
        }

        public void AddShip(IShip s)
        {
            _model.OwnedShipIds.Add(s.Id);
        }

        /// <summary>
        /// Sets ActiveShipId to null if shipId == ActiveShipId
        /// </summary>
        /// <param name="shipId"></param>
        public void RemoveShip(int shipId)
        {
            _model.OwnedShipIds.Remove(shipId);
            if (_model.ActiveShipId == shipId)
                _model.ActiveShipId = null;
        }

        public virtual Account GetAccount()
        {
            return _accountLocator.GetAccountAsync(_model.AccountID).Result;
        }

        public IShip GetActiveShip()
        {
            return _shipLocator.GetShipAsync(_model.ActiveShipId).Result;//I don't expect this to deadlock, because it doesn't call to the db and nothing is awaited.
        }

        public virtual IArea GetArea()
        {
            return _areaLocator.GetArea(_model.CurrentAreaID);
        }

        public Player SetPlanetTeam(Team t)
        {
            _model.DefaultTeamID = t.Id;
            return this;
        }

        /// <summary>
        /// Adds Team t to the player's list of teams.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Player AddTeam(Team t)
        {
            _model.TeamIDs.Add(t.Id);
            return this;
        }

        public Player RemoveTeam(int teamID)
        {
            _model.TeamIDs.Remove(teamID);
            return this;
        }

        public HashSet<int> GetTeamIDs()
        {
            return new HashSet<int>(_model.TeamIDs);
        }

        public IDBObject GetDBObject()
        {
            return _model.GetClone();
        }
    }    
        
}
