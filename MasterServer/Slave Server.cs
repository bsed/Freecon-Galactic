using System;
using System.Collections.Generic;
using Freecon.Core.Utils;

namespace MasterServer
{

    public class SlaveServer
    {
        HashSet<int> _mySystems = new HashSet<int>();
        HashSet<int> _myShips = new HashSet<int>();

        public int ShipCount { get { return _myShips.Count; } }
        public int SystemCount { get { return _mySystems.Count; } }
        int _ID;
        public int ID { get { return _ID; } }
             

        //Areas assigned to slave for simulation
        public HashSet<int> AssignedAreas = new HashSet<int>();

        public float LastPingTimestamp;

        public float InitializationTime;
        
        public SlaveServer(int slaveID)
        {
            _ID = slaveID;
            LastPingTimestamp = TimeKeeper.MsSinceInitialization;
            InitializationTime = TimeKeeper.MsSinceInitialization;
        }        
        
        /// <summary>
        /// Returns true if galaxyID is assigned to this server, false otherwise
        /// </summary>
        /// <param name="galaxyID"></param>
        /// <returns></returns>
        public bool AssignedID(int galaxyID)
        {
            return _myShips.Contains(galaxyID) || _mySystems.Contains(galaxyID);

        }

        public void AddShip(int shipID)
        {
            _myShips.Add(shipID);
        }

        public void RemoveShip(int shipID)
        {
            _myShips.Remove(shipID);
        }

        public void AddSystem(int systemID)
        {
            _mySystems.Add(systemID);
        }

        public void RemoveSystem(int systemID)
        {
            _mySystems.Remove(systemID);
        }

        /// <summary>
        /// Notifies this server to cease updating the given systemIDs
        /// </summary>
        /// <param name="systemIDs"></param>
        public void NotifyNewIDsToUpdate(List<int> galaxyIDs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Notifies this server to cease updating the given systemIDs
        /// </summary>
        /// <param name="systemIDs"></param>
        public void NotifyStopUpdatingIDs(List<int> galaxyIDs)
        {
            throw new NotImplementedException();
        }

        public void ClearMySystems()
        {
            _mySystems.Clear();
        }

        /// <summary>
        /// Returns a list of all IDs that this slave is handling
        /// </summary>
        /// <returns></returns>
        public List<int> GetSystemIDs()
        {
            List<int> IDs = new List<int>(SystemCount);
            foreach (int i in _mySystems)
                IDs.Add(i);

            return IDs;

        }
                
    }
}
