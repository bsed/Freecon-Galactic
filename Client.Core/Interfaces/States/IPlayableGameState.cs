using Freecon.Client.Core.States.Components;
using Freecon.Client.Objects.Structures;

namespace Freecon.Client.Core.Interfaces.States
{
    public interface IPlayableGameState : IGameState
    {
        void AddStructure(Structure s);

        void KillStructure(int ID);

        IStructureManager StructureManager { get; }
    }
}
