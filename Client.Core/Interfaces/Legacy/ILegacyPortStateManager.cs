using Freecon.Client.Core.Objects;
using System.Collections.Generic;

namespace Freecon.Client.Core.Interfaces.Legacy
{
    public interface ILegacyPortStateManager : ILegacyGameState
    {
        void Clear();

        Dictionary<int, PortShip> ShipsInPort { get; set; }

        Dictionary<byte, PortService> OutfitForSale { get; set; }
    }
}
