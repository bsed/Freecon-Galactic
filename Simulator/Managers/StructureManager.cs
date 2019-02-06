using Freecon.Client.Objects.Structures;
using Core.Interfaces;
using Freecon.Client.Core.Interfaces;
using Freecon.Models.TypeEnums;
using System.Collections.Generic;

namespace Simulator.Managers
{
    public class StructureManager:IStructureManager
    {

        public IEnumerable<Structure> Structures { get { return _structureCache; } }
        HashSet<Structure> _structureCache;//TODO: temporary, remove this and change IEnumerable IStructureManager::Structures to IReadOnlyDictionary
        Dictionary<int, Structure> _structures;

        HashSet<Turret> _turrets;

        public StructureManager()
        {
            _structureCache = new HashSet<Structure>();
            _structures = new Dictionary<int, Structure>();
        }

        public void AddStructure(Structure structure)
        {
            if (!_structures.ContainsKey(structure.Id))
            {
                _structures.Add(structure.Id, structure);
                _structureCache.Add(structure);

                if (structure.StructureType == StructureTypes.LaserTurret)
                    _turrets.Add((Turret)structure);
            }


        }


        public void RemoveStructure(Structure structure)
        {
            if (_structures.ContainsKey(structure.Id))
            {
                _structures.Remove(structure.Id);
                _structureCache.Remove(structure);

                if (structure.StructureType == StructureTypes.LaserTurret)
                    _turrets.Remove((Turret)structure);
            }
        }

        public Structure GetStructureByID(int structureID)
        {
            if (_structures.ContainsKey(structureID))
                return _structures[structureID];
            else
                return null;
        }

        public IEnumerable<Turret> FindAllTurrets()
        {
            return _turrets;
        }
 
        public void Clear()
        {
            _structureCache.Clear();
            _structures.Clear();
            _turrets.Clear();
        }
    
        public void Update(IGameTimeService gameTime)
        {
            foreach (var kvp in _structures)
                kvp.Value.Update(gameTime);
        }

    }
}
