using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Core.Interfaces;
using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.Managers
{
    /// <summary>
    /// Used to simulate network items when commanded by the server
    /// </summary>
    public class SimulationManager : ISynchronousUpdate
    {
        private Dictionary<int, ISimulatable> _objectsToSimulate = new Dictionary<int, ISimulatable>();

        public SimulationManager()
        {

#if DEBUG
            Debugging.SimulationManager = this;
#endif
        }

        List<ISimulatable> toRemove = new List<ISimulatable>();
        
        public void StartSimulating(ISimulatable obj)
        {
            if (!_objectsToSimulate.ContainsKey(obj.Id))
            {
                _objectsToSimulate.Add(obj.Id, obj);
                obj.IsLocalSim = true;
            }            
        }

        public void StopSimulating(ISimulatable obj)
        {
            if (_objectsToSimulate.ContainsKey(obj.Id))
                _objectsToSimulate.Remove(obj.Id);

            obj.IsLocalSim = false;

        }

        public void Update(IGameTimeService gameTime)
        {
            toRemove.Clear();
            foreach (var kvp in _objectsToSimulate)
            {
                if (!kvp.Value.IsBodyValid)
                {   
                    toRemove.Add(kvp.Value);
                    continue;
                }
                kvp.Value.Simulate(gameTime);
                
            }

            // This is probably a bad idea and I'll get rid of it later.
            foreach(ISimulatable s in toRemove)
                _objectsToSimulate.Remove(s.Id);
            

        }

        public void Clear()
        {
            foreach(var v in _objectsToSimulate)
            {
                v.Value.IsLocalSim = false;
            }

            _objectsToSimulate.Clear();
        }


    }
}
