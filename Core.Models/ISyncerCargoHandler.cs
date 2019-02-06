using Core.Models;
using Core.Models.CargoHandlers;
using Freecon.Models.TypeEnums;

namespace Freecon.Models
{
    public interface ISyncerCargoHandler
    {
        CargoResult RemoveStatelessCargo(StatelessCargoTypes type, float numToRemove);

        CargoResult AddStatelessCargo(StatelessCargoTypes type, float numToAdd, bool suspendBoundsChecking);

        StatefulCargo RemoveStatefulCargo(int id);

        CargoResult AddStatefulCargo(StatefulCargo c, bool suspendBoundsChecking);

        StatefulCargo RemoveStatefulCargo(StatefulCargoTypes type);
                
    }
}
