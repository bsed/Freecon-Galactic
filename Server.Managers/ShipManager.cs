using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using SRServer.Services;
using Freecon.Core.Networking.Models;
using Server.Models;
using Core.Models;
using Server.Database;
using System.Threading.Tasks;
using Freecon.Models.TypeEnums;
using Server.Interfaces;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Utils;

namespace Server.Managers
{
    public class ShipManager:IShipLocator
    {
        object SHIPS_LOCK = new object();

        ConcurrentDictionary<int, IShip> _ships; //All of the ships in the galaxy. Convenient for IShip lookups.

        MessageManager _messageManager;
        IAreaLocator _areaLocator;
        WarpManager _warpManager;
        ConnectionManager _connectionManager;
        IDatabaseManager _databaseManager;
        ILocalIDManager _galaxyIdManager;
            

        public ShipManager(MessageManager mm, IAreaLocator gm, WarpManager wm, ConnectionManager cm, IDatabaseManager dbm)
        {
            _messageManager = mm;
            _areaLocator = gm;
            _warpManager = wm;
            _connectionManager = cm;
            _databaseManager = dbm;

            _ships = new ConcurrentDictionary<int, IShip>();
            
        }

        public void Update()
        {
            foreach (var kvp in _ships)
            {
                IShip s = kvp.Value;

                float oldHealth = s.CurrentHealth;
                s.Update();


                if (oldHealth != s.CurrentHealth && TimeKeeper.MsSinceInitialization - s.LastHealthUpdateTime >= s.HealthUpdatePeriod)
                {
                    var data = new MessageSetHealth();
                    data.HealthData.Add(new HealthData() { Health = (int)s.CurrentHealth, Shields = (int)s.Shields.CurrentShields, Energy = (int)s.CurrentEnergy });

                   s.GetPlayer().SendMessage(new NetworkMessageContainer(data, MessageTypes.SetHealth));

                    s.LastHealthUpdateTime = TimeKeeper.MsSinceInitialization;                
                }

                if(s.IsDead)
                {
                    if (TimeKeeper.MsSinceInitialization - s.KillTimeStamp > s.RespawnTimeDelay)
                    {
                        ReviveShip(s.Id, _areaLocator.GetArea(s.ReviveAreaID==null?_areaLocator.SolAreaID:s.ReviveAreaID), .2f, 0);
                    }
                }
            }

        }

        public void RegisterShip(IShip s)
        {
            lock (SHIPS_LOCK)
            {
                _ships.AddOrUpdate(s.Id, s, (k, v) => s);
                
            }
        }       

        public void RemoveShip(IShip ship)
        {
            if (ship == null)
                return;

            IShip tempShip;

            lock (SHIPS_LOCK)
            {                
                _ships.TryRemove(ship.Id, out tempShip);
            }
        }

        /// <summary>
        /// Changes IShip and notifies client.
        /// </summary>
        /// <param name="s">Ship of the player</param>
        /// <param name="shs">Ship to switch to</param>
        /// <param name="w">Weapon to switch to</param>
        public void ChangePlayersShip(IShip s, ShipStats shs)
        {
            s.ChangeShipType(shs);
            var data = s.GetNetworkData(false, false, false, true);
            s.GetPlayer().SendMessage(new NetworkMessageContainer(data, MessageTypes.ChangeShipType)); 
        }
               
        public void DeregisterShip(int ID)
        {
            IShip s;
            _ships.TryRemove(ID, out s);

        }
        /// <summary>
        /// This will probably be removed, just for testing db 
        /// </summary>
        /// <returns></returns>
        public List<IShip> GetAllShips()
        {
            return _ships.Values.ToList();

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="fetchFromDB">If true, ls cannot be null</param>
        /// <param name="ls">Cannot be null if fetchFromDB is true</param>
        /// <param name="persistFetch">If true, fetched instance is stored/updated in the manager's list of ships</param>
        /// <returns></returns>
        public async Task<IShip> GetShipAsync(int? ID, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true)
        {
            if (ID == null)
                return null;

            IShip s = null;//Default value if s isn't found

            if (fetchFromDB)
            {
                if(ls == null)
                {
                    throw new Exception("Cannot fetch a IShip from the database without a reference to a LocatorService object.");
                }

                var sm = await _databaseManager.GetShipAsync((int)ID);
                s = _instantiateShip(sm, ls);

               
                if (persistFetch && s != null)
                {
                    RegisterShip(s);
                }
            }
            else
            {
                if (_ships.ContainsKey((int)ID))
                    s = _ships[(int)ID];
            }

            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="fetchFromDB">If true, ls cannot be null</param>
        /// <param name="ls">Cannot be null if fetchFromDB is true</param>
        /// <param name="persistFetch">If true, fetched instance is stored/updated in the manager's list of ships</param>
        /// <returns></returns>
        public async Task<ICollection<IShip>> GetShipsAsync(IEnumerable<int> IDs, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true)
        {
            var retShips = new List<IShip>();

            if (fetchFromDB)
            {
                if (ls == null)
                {
                    throw new Exception("Cannot fetch a IShip from the database without a reference to a LocatorService object.");
                }

                var shipModels = await _databaseManager.GetShipsAsync(IDs);

                shipModels.Select(s => _instantiateShip(s, ls)).ForEach(newShip =>
                {
                    retShips.Add(newShip);

                    if (persistFetch)
                    {
                        RegisterShip(newShip);
                    }
                });
            }
            else
            {
                retShips.AddRange(
                    IDs.Where(id => _ships.ContainsKey(id)).Select(id => _ships[id])
                );
            }

            return retShips;
        }

        /// <summary>
        /// Returns false if the ship is not found, true otherwise.
        /// </summary>
        public bool UpdateShip(PositionUpdateData ud)
        {
            if (!_ships.ContainsKey(ud.TargetId))
            {
                return false;
            }

            var s = _ships[ud.TargetId];

            s.PosX = ud.XPos;
            s.PosY = ud.YPos;
            s.Rotation = ud.Rotation;
            s.VelX = ud.XVel;
            s.VelY = ud.YVel;
            s.AngVel = ud.AngularVelocity; 
            s.Thrusting = ud.Thrusting; 

            return true;
        }

        private IShip _instantiateShip(ShipModel s, LocatorService ls)
        {
            switch (s.PilotType)
            {
                case PilotTypes.Player:
                    return new PlayerShip((PlayerShipModel)s, ls);                    

                case PilotTypes.NPC:
                    return new NPCShip((NPCShipModel)s, ls);

                case PilotTypes.Simulator:
                    throw new NotImplementedException("Simulator Pilot Type not handled.");

                default:
                    throw new NotImplementedException();
            }
        }

        //public IShip KillShip(int? shipId, int projectileID)
        //{
        //    if (shipId == null || !_allShips.ContainsKey((int)shipId))
        //        return null;
        //    else
        //        return KillShip(_allShips[(int)shipId], projectileID);

        //}


        public IShip ReviveShip(int? shipId, IArea sendHere, float healthMultiplier, float shieldsMultipler)
        {
            if(shipId != null && _ships.ContainsKey((int)shipId))
            {
                return ReviveShip(_ships[(int)shipId], sendHere, healthMultiplier, shieldsMultipler);
            }

            return null;
        }

        /// <summary>
        /// Revives a player and sends him to sendHere
        /// </summary>
        public IShip ReviveShip(IShip s, IArea sendHere, float healthMultiplier, float shieldsMultipler)
        {
            s.CurrentHealth = s.ShipStats.MaxHealth;
            s.IsDead = false;
            _warpManager.ChangeArea(sendHere.Id, s, false, sendHere.AreaType == AreaTypes.System);

            if (!s.GetPlayer().IsOnline)
            {
                return s;
            }

            var health = (int)(s.MaxHealth * healthMultiplier);
            var shields = (int)(s.MaxShields * shieldsMultipler);

            _messageManager.SendReviveMessage(s, health, shields);

            return s;
        }

    }
}
