using Freecon.Client.Core.Interfaces;
using Freecon.Models.TypeEnums;
using Core.Interfaces;
using Freecon.Client.Objects.Structures;
using System.Collections.Generic;
using System.Linq;

namespace Freecon.Client.ViewModel
{
    public abstract class PlayableViewModel : IViewModel, IStructureManager
    {
        protected virtual IList<Structure> _structures { get; set; }

        public virtual IEnumerable<Structure> Structures { get { return _structures; } }

        public PlayableViewModel()
        {
            _structures = new List<Structure>();
        }

        public void AddStructure(Structure structure)
        {
            _structures.Add(structure);
        }

        public void RemoveStructure(Structure structure)
        {
            if (structure.IsBodyValid)
            {
                structure.DisposeBodies();
            }
            _structures.Remove(structure);
        }

        /// <summary>
        /// Returns a reference to the structure with the passed ID if it exists, null otherwise
        /// </summary>
        /// <param name="structureID"></param>
        /// <returns></returns>
        public Structure GetStructureByID(int structureID)
        {
            return _structures.FirstOrDefault(p => p.Id == structureID);
        }

        public virtual void Clear()
        {
            _structures.Clear();
        }

        public IEnumerable<Turret> FindAllTurrets()
        {
            return this._structures.Where(p => p.StructureType == StructureTypes.LaserTurret).Select(p => p as Turret).Where(p => p != null);
        }

        public virtual void Update(IGameTimeService gameTime)
        {
            foreach (var structure in _structures)
            {
                structure.Update(gameTime);
            }
        }
    }
}
