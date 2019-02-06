using Freecon.Client.Objects.Structures;
using Core.Interfaces;
using System.Collections.Generic;

namespace Freecon.Client.Core.Interfaces
{
    public interface IStructureManager:ISynchronousUpdate
    {
        IEnumerable<Structure> Structures { get; }

        void AddStructure(Structure structure);

        void RemoveStructure(Structure structure);

        Structure GetStructureByID(int structureID);

        IEnumerable<Turret> FindAllTurrets();

        void Update(IGameTimeService gameTime);

        void Clear();
    }
}
