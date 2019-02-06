using System.Collections.Generic;
using System.Linq;
using Freecon.Client.Interfaces;
using Freecon.Client.Extensions;
using Freecon.Client.Objects;
using Freecon.Client.Objects.Structures;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Objects.Invasion;
using Server.Managers;

namespace Freecon.Client.Managers
{
    /// <summary>
    /// Manages clientside targetting of all ITargetable by all ICanTarget
    /// </summary>
    public class TargetingService
    {
        private Dictionary<int, ITargetable> _targetableObjects = new Dictionary<int, ITargetable>();
        private Dictionary<int, ITargeter> _targetingObjects = new Dictionary<int, ITargeter>();

        private bool _suspendTargetSetting = false;

        private void RegisterTarget(ITargetable target)
        {
            if (target is DefensiveMine)
                return;

            if (!_targetableObjects.ContainsKey(target.Id))
            {
                _targetableObjects.Add(target.Id, (target));
                if(!_suspendTargetSetting)
                    SetForNewTargetable(target);
            }
        }

        private void DeRegisterTarget(ITargetable target)
        {
            var targetsToDeregister = _targetingObjects
                .Select(kvp => kvp.Value)
                .Where(k => k.PotentialTargets.ContainsKey(target.Id));

            foreach (var targeting in targetsToDeregister)
            {
                targeting.PotentialTargets.Remove(target.Id);

                if (targeting.CurrentTarget == target)
                {
                    targeting.CurrentTarget = null;
                }
            }

            if (_targetableObjects.ContainsKey(target.Id))
            {
                _targetableObjects.Remove(target.Id);
            }
        }

        private void RegisterTargeter(ITargeter targeter)
        {
            if (targeter is DefensiveMine)
                return;

            if (!_targetingObjects.ContainsKey(targeter.Id))
            {
                _targetingObjects.Add(targeter.Id, targeter);

                if (!_suspendTargetSetting)
                {
                    SetPotentialTargets(targeter);
                }
            }
        }

        private void DeRegisterTargeter(ITargeter targeter)
        {
            targeter.CurrentTarget = null;

            if (!(targeter is Turret && ((Turret)targeter).TurretType == TurretTypes.Planet))
            {
                targeter.PotentialTargets.Clear();
            }

            if (_targetingObjects.ContainsKey(targeter.Id))
            {
                _targetingObjects.Remove(targeter.Id);
            }
        }

        /// <summary>
        /// Returns an ITargetable if the ID is registered, null otherwise
        /// </summary>
        /// <param name="ID"></param>
        public ITargetable GetTargetableObject(int ID)
        {
            if (_targetableObjects.ContainsKey(ID))
                return _targetableObjects[ID];
            else
            {
                ConsoleManager.WriteLine("Target not found in TargetingManager.GetTargetable()", ConsoleMessageType.Error);
                return null;
            }
        }

        /// <summary>
        /// Automatically registers both ICanTargets and ITargetables appropriately 
        /// </summary>
        /// <param name="obj"></param>
        public void RegisterObject(object obj)
        {
            var target = obj as ITargetable;
            if (target != null)
            {
                RegisterTarget(target);               
            }

            var targeter = obj as ITargeter;
            if (targeter != null)
            {
                RegisterTargeter(targeter);
            }
        }

        /// <summary>
        /// Automatically registers both ICanTargets and ITargetables appropriately 
        /// </summary>
        /// <param name="obj"></param>
        public void DeRegisterObject(object obj)
        {
            if (obj is ITargetable)
            {
                DeRegisterTarget((ITargetable)obj);
            }

            if (obj is ITargeter)
            {
                DeRegisterTargeter((ITargeter)obj);
            }
        }

        /// <summary>
        /// Iterates through _targetableObjects and sets targeter.PotentialTargets appropriately
        /// </summary>
        /// <param name="targeter"></param>
        private void SetPotentialTargets(ITargeter targeter)
        {
            // If landing becomes slow for large numbers of turrets, 
            // Reimplement this with single PotentialTurretTargets collection.
            targeter.PotentialTargets.Clear();

            foreach(var pt in _targetableObjects)
            {
                if (pt.Value is CommandCenter)
                {
                    continue;
                }
                else if (!pt.Value.OnSameTeam(targeter))
                {
                    targeter.PotentialTargets.Add(pt.Value.Id, pt.Value);
                }
            }

        }

        /// <summary>
        /// Iterates through _targetingObjects and adds target to each ICanTarget.PotentialTargets, if appropriate.
        /// Useful when adding a new item to a running state (e.g. ship warps in)
        /// </summary>
        /// <param name="target"></param>
        private void SetForNewTargetable(ITargetable target)
        {
            foreach (var pt in _targetingObjects)
                if (!pt.Value.OnSameTeam(target) && !pt.Value.PotentialTargets.ContainsKey(target.Id))
                    pt.Value.PotentialTargets.Add(target.Id, _targetableObjects[target.Id]);
        }

        /// <summary>
        /// Resets targeting for the passed object, depending on whether it is ITargetable, ITargeter, or both
        /// </summary>
        /// <param name="obj"></param>
        public void ResetTargeting(object obj)
        {
            if(obj is ITargeter)
                SetPotentialTargets((ITargeter)obj);

            if(obj is ITargetable)
                SetForNewTargetable((ITargetable)obj);

        }

        public bool IsIDRegistered(int ID)
        {
            return (_targetableObjects.ContainsKey(ID) || _targetingObjects.ContainsKey(ID));

        }   

        /// <summary>
        /// Pass the playership if it should be preserved, pass null otherwise
        /// </summary>
        /// <param name="playerShip"></param>
        public void Clear(Ship playerShip)
        {
            
            ConsoleManager.WriteLine("Clearing TargetingManager", ConsoleMessageType.ManagerRegistration);
            _targetingObjects.Clear();
            _targetableObjects.Clear();

            if (playerShip != null)
                RegisterObject(playerShip);

        }

        /// <summary>
        /// Suspends setting of targets for one update when simultaneously registering large numbers of objects
        /// E.G. reading incoming system with many turrets/ships
        /// ENABLETARGETSETTING MUST BE CALLED WHEN LOADING IS COMPLETE if targeting is required in the state (E.G. no need in ports)
        /// </summary>
        public void DisableTargetSetting()
        {
            // If remembering to call EnableTargetSetting() proves too difficult,
            // A timeout may be enstated, however this could lead to sloppy latency or buggy behavior on slow computers.
            _suspendTargetSetting = true;
        }

        /// <summary>
        /// Enables target setting and sets targets for all registered ITargeter objects
        /// </summary>
        public void EnableTargetSetting()
        {
            _suspendTargetSetting = false;

            ResetTargeting();            
            
        }

        public void ResetTargeting()
        {
            foreach (var kvp in _targetingObjects)
            {
                kvp.Value.PotentialTargets.Clear();
            }

            foreach (var kvp in _targetableObjects)
            {
                SetForNewTargetable(kvp.Value);
            }

            foreach (var kvp in _targetingObjects)
            {
                if (kvp.Value.CurrentTarget != null && !kvp.Value.PotentialTargets.ContainsKey(kvp.Value.CurrentTarget.Id))
                    kvp.Value.CurrentTarget = null;
            }
        }
    }
}
