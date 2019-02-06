using System.Collections.Generic;
using Freecon.Client.Interfaces;

namespace Freecon.Client.Managers
{
    public class TeamManager
    {
        private Dictionary<int, ITeamable> _teamableObjects = new Dictionary<int, ITeamable>();
        
        private TargetingService _targetingManager;


        public TeamManager(TargetingService targetingManager)
        {
            _targetingManager = targetingManager;
        }         
                
        /// <summary>
        /// Registers a new ITeamable, if it is not already registered.
        /// Handles registration with TargetingManager if necessary
        /// </summary>
        /// <param name="t"></param>
        public void RegisterObject(ITeamable t)
        {
            if (!_teamableObjects.ContainsKey(t.Id))
            {
                _teamableObjects.Add(t.Id, t);
            }
            
        }

        /// <summary>
        /// Deregisters ITeamable t, if it is currently registered
        /// </summary>
        /// <param name="t"></param>
        public void DeRegisterObject(ITeamable t)
        {
            //_targetingManager.DeRegisterObject(t);

            if (_teamableObjects.ContainsKey(t.Id))
                _teamableObjects.Remove(t.Id);


        }

        /// <summary>
        /// Adds the TeamNumber to the list of teams for the object with the given ID. Object must be registered with teammanager, otherwise an exception will be thrown
        /// Resets targeting for all objects
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="TeamNumber"></param>
        public void AddTeamToObject(int ID, int teamNumber)
        {
            _teamableObjects[ID].Teams.Add(teamNumber);
            _targetingManager.ResetTargeting(_teamableObjects[ID]);
        }

        /// <summary>
        /// Removes the TeamNumber from the list of teams of the object with the given ID. Object must be registered with teammanager, otherwise an exception will be thrown
        /// Resets targeting for all objects
        /// </summary>
        public void RemoveTeamFromObject(int ID, int teamNumber)
        {
            _teamableObjects[ID].Teams.Remove(teamNumber);
            _targetingManager.ResetTargeting(_teamableObjects[ID]);
        }

        public void Clear()
        {
            _teamableObjects.Clear();
        }

    }
}
