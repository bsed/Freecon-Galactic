using Core.Models;
using Freecon.Models.TypeEnums;

namespace Server.Models.Interfaces
{
    public interface ITransactionAddStatefulCargo
    {
        StatefulCargoTypes CargoType { get; }

        StatefulCargo CargoObject { get; }

        IHasCargo CargoHolder { get; }

    }

    public interface ITransactionRemoveStatefulCargo
    {
        StatefulCargoTypes CargoType { get; }

        StatefulCargo RemovedCargo { get; }

        int? CargoID { get; }

        IHasCargo CargoHolder { get; }

    }

    public interface ITransactionAddStatelessCargo
    {
        StatelessCargoTypes CargoType { get; }

        float Quantity { get; }

        IHasCargo CargoHolder { get; }

    }

    public interface ITransactionRemoveStatelessCargo
    {
        IHasCargo CargoHolder { get; }

        StatelessCargoTypes CargoType { get; }

        float Quantity { get; }

        object OnCompletionData { get; }
    }
}
