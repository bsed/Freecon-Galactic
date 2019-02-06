using Freecon.Client.Core.Interfaces.States;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Freecon.Client.Core.Interfaces
{
    public interface IPlanetStateManager : IPlayableGameState
    {
        HashSet<int> ColonyTeamIDs { get; }

        void KillStructure(int ID);

        void LoadPlanetLevel(PlanetTypes planetType, IEnumerable<IEnumerable<Vector2>> islands, int height, int width, bool[] layoutArray);

        void LoadTestPlanet(PlanetTypes planetType);
    }
}
