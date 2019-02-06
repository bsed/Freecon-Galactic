using Freecon.Models;
using Server.Managers.Synchronizers.Transactions;
using Core.Models.CargoHandlers;
using Server.Models;
using Server.Managers.Synchronizers;

namespace Server.Managers
{

    //This is kind of an experiment in synchronizing and atomicizing a multithreaded application
    //TODO: Adapt to generic Synchronizer<T>
    //Apparently I've rediscovered the actor model?
    public class CargoSynchronizer : Synchronizer<CargoTransaction, CargoTransactionSequence, ISyncerCargoHandler, CargoResult>
    {

        protected override void SetViewModel(ISynchronousTransaction<ISyncerCargoHandler, CargoResult> transaction)
        {
            var port = transaction.CargoHolder as Port;
            ISyncerCargoHandler sch;
            if (port == null)
            {
                sch = new CargoHandler_ReadAddRemoveVM<CargoHandlerModel>(transaction.CargoHolder.GetCargo());
            }
            else
            {
                var ch = port.GetCargo();
                sch = new CargoHandlerPort_SyncerVM(new CargoHandlerPort_ROVM(ch), port.PriceGetter, port.PriceGetter);
            }

            transaction.SetSyncherVM(sch);

        }

    }
}


