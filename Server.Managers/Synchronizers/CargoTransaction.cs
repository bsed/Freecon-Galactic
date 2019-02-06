using Core.Models;
using Core.Models.CargoHandlers;
using Freecon.Models;
using Freecon.Models.TypeEnums;
using Server.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Managers.Synchronizers.Transactions
{
    public abstract class CargoTransaction : ICargoTransaction, ISynchronousTransaction<ISyncerCargoHandler, CargoResult>
    {
        public readonly int CargoHolderID;

        public IHasCargo CargoHolder { get { return _cargoHolder; } }
        readonly IHasCargo _cargoHolder;
        
        public bool IsProcessed { get; set; }
        public CargoTransactionTypes TransactionType { get; protected set; }

        //Allows a consumer to await the result, which is processed in another thread
        public Task<CargoResult> ResultTask { get; protected set; }
        public TaskCompletionSource<CargoResult> ResultCompletionSource { get; set; }

        protected ISyncerCargoHandler _cargoHandler;


        public CargoResult SequenceFailedValue { get { return CargoResult.SequenceFailed; } }
        public CargoResult IdLockAttemptTimeoutValue { get { return CargoResult.IDLockAttemptTimeout; } }
        public CargoResult SuccessValue { get { return CargoResult.Success; } }

        public object UniqueLockableId { get { return _cargoHolder.Id; } }

        /// <summary>
        /// Kind of gross, but convenient.
        /// </summary>
        public object OnCompletionData { get; set; }

        /// <summary>
        /// Automatically set to true if the transaction is part of a sequence
        /// </summary>
        public bool IsInSequence { get; internal set; }
               
        public CargoTransaction(IHasCargo subject, CargoTransactionTypes type)
        {
            _cargoHolder = subject;
            CargoHolderID = subject.Id;
            TransactionType = type;

            ResultCompletionSource = new TaskCompletionSource<CargoResult>();
            ResultTask = ResultCompletionSource.Task;
        }

        public void SetSyncherVM(ISyncerCargoHandler cargoHandlerVM)
        {
            _cargoHandler = cargoHandlerVM;
        }

        public abstract CargoResult Process();

        public abstract void Rollback();

        public abstract void CloseTransaction();

    }

    public class TransactionAddStatelessCargo : CargoTransaction, ITransactionAddStatelessCargo
    {
        public StatelessCargoTypes CargoType { get; protected set; }
        public float Quantity { get; protected set; }

        public readonly bool suspendBoundsChecking;

        /// <summary>
        /// Called if transaction is succesful. Both args are this transaction
        /// </summary>
        public EventHandler<ITransactionAddStatelessCargo> OnCompletion;
                
        public TransactionAddStatelessCargo(IHasCargo recipient, StatelessCargoTypes cargoType, float amount, bool suspendBoundsChecking):base(recipient, CargoTransactionTypes.AddStatelessCargo)
        {
            CargoType = cargoType;
            Quantity = amount;
        }

        public override CargoResult Process()
        {

            ResultCompletionSource.SetResult(_cargoHandler.AddStatelessCargo(CargoType, Quantity, suspendBoundsChecking));                  
                
            if(!IsInSequence && ResultTask.Result == CargoResult.Success)
            {
                CloseTransaction();
            }
            return ResultTask.Result;
        }

        public override void Rollback()
        {
            if (!ResultTask.IsCompleted)
            {
                throw new Exception("Error: attempted to rollback a transaction before it was processed.");
            }

            if (ResultTask.Result != CargoResult.Success)
            {
                return;//This state probably isn't valid, but just in case...
            }

            _cargoHandler.RemoveStatelessCargo(CargoType, Quantity);
            ResultCompletionSource.SetResult(CargoResult.RolledBack);
        }

        /// <summary>
        /// Called when the transaction, or a sequence of transactions, is/are completed
        /// </summary>
        public override void CloseTransaction()
        {
            if(OnCompletion != null)
            {
                OnCompletion(this, this);
            }
        }


    }

    /// <summary>
    /// Add the given StatefulCargo
    /// </summary>
    public class TransactionAddStatefulCargo : CargoTransaction, ITransactionAddStatefulCargo
    {
        public StatefulCargoTypes CargoType { get; protected set; }

        public StatefulCargo CargoObject { get; set; }//Should be readonly, this is leaky, but convenient for chaining transactions (remove from A, give to B)

        public readonly bool suspendBoundsChecking;

        /// <summary>
        /// Called if transaction is succesful. Both args are this transaction
        /// </summary>
        public EventHandler<ITransactionAddStatefulCargo> OnCompletion;
        
        /// <summary>
        /// Leave cargoObject null to chain to a previous transaction in a sequence.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="cargoObject"></param>
        /// <param name="suspendBoundsChecking"></param>
        public TransactionAddStatefulCargo(IHasCargo recipient, StatefulCargo cargoObject, bool suspendBoundsChecking):base(recipient, CargoTransactionTypes.AddStatefulCargo)
        {
            if(cargoObject != null)
                CargoType = cargoObject.CargoType;
            

            CargoObject = cargoObject;
        }

        public override CargoResult Process()
        {
            if (CargoObject == null)
                throw new Exception("Error: CargoObject not set in TransactionAddStatefulCargo before processing.");

            ResultCompletionSource.SetResult(_cargoHandler.AddStatefulCargo(CargoObject, suspendBoundsChecking));
            if(!IsInSequence && ResultTask.Result == CargoResult.Success)
            {
                CloseTransaction();
            }
            return ResultTask.Result;
        }

        public override void Rollback()
        {
            if (!ResultTask.IsCompleted)
            {
                throw new Exception("Error: attempted to rollback a transaction before it was processed.");
            }

            if (ResultTask.Result != CargoResult.Success)
            {
                return;//This state probably isn't valid, but just in case...
            }

            _cargoHandler.RemoveStatefulCargo(CargoObject.Id);
            ResultCompletionSource.SetResult(CargoResult.RolledBack);
        }

        public override void CloseTransaction()
        {
            if(OnCompletion != null)
            {
                OnCompletion((ITransactionAddStatefulCargo)this, this);
            }
        }
    }

    public class TransactionRemoveStatelessCargo : CargoTransaction, ITransactionRemoveStatelessCargo
    {
        public StatelessCargoTypes CargoType { get; protected set; }

        public float Quantity { get; protected set; }
        
        public EventHandler<ITransactionRemoveStatelessCargo> OnCompletion;

        public TransactionRemoveStatelessCargo(IHasCargo donor, StatelessCargoTypes cargoType, float amount):base(donor, CargoTransactionTypes.RemoveStatelessCargo)
        {
            CargoType = cargoType;
            Quantity = amount;
        }

        public override CargoResult Process()
        {
            ResultCompletionSource.SetResult(_cargoHandler.RemoveStatelessCargo(CargoType, Quantity));
            if(!IsInSequence && ResultTask.Result == CargoResult.Success)
            {
                CloseTransaction();
            }
            return ResultTask.Result;
        }

        public override void Rollback()
        {
            if (!ResultTask.IsCompleted)
            {
                throw new Exception("Error: attempted to rollback a transaction before it was processed.");
            }

            if (ResultTask.Result != CargoResult.Success)
            {
                return;//This state probably isn't valid, but just in case...
            }
            _cargoHandler.AddStatelessCargo(CargoType, Quantity, true);
            ResultCompletionSource.SetResult(CargoResult.RolledBack);
        }

        public override void CloseTransaction()
        {
            if(OnCompletion != null)
                OnCompletion(this, this);
        }
    
    }    

    public class TransactionRemoveStatefulCargo : CargoTransaction, ITransactionRemoveStatefulCargo
    {
        public StatefulCargoTypes CargoType { get; protected set; }

        /// <summary>
        /// Set to null if the transaction should take the first Cargo matching CargoType, in which case this parameter is automatically set on completion 
        /// </summary>
        public int? CargoID { get; protected set; }

        /// <summary>
        /// This will be non-null if the transaction is succesful
        /// </summary>
        public StatefulCargo RemovedCargo { get; protected set; }

        public EventHandler<ITransactionRemoveStatefulCargo> OnCompletion;
        
        /// <summary>
        /// Use this to transfer cargo from one entity to another.
        /// </summary>
        public readonly bool TransferToNextTransaction;

        /// <summary>
        /// Set cargoID to null if transaction applies to first cargo found in IHasCargo object    
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="cargoType"></param>
        /// <param name="cargoID"></param>
        /// <param name="transferToNextTransaction">If true, and this transaction occurs as part of a SequentialTransaction, and the next transaction is of type AddStatefulCargo, then the cargo object removed by this transaction will serve as the Cargo added in the subsequent AddStatefulCargoTransaction</param>
        public TransactionRemoveStatefulCargo(IHasCargo donor, StatefulCargoTypes cargoType, int? cargoID, bool transferToNextTransaction = false) : base(donor, CargoTransactionTypes.RemoveStatefulCargo)
        {
            CargoType = cargoType;
            CargoID = cargoID;
            TransferToNextTransaction = transferToNextTransaction;
        }


        public override CargoResult Process()
        {
            if (CargoID != null)
            {
                RemovedCargo = _cargoHandler.RemoveStatefulCargo((int)CargoID);               
            }
            else
            {
                RemovedCargo = _cargoHandler.RemoveStatefulCargo(CargoType);
            }
            if (RemovedCargo != null)
                ResultCompletionSource.SetResult(CargoResult.Success);
            else
                ResultCompletionSource.SetResult(CargoResult.CargoNotInHolds);


            if(!IsInSequence && ResultTask.Result == CargoResult.Success)
            {
                CloseTransaction();
            }
            return ResultTask.Result;
        }

        public override void Rollback()
        {
            if (!ResultTask.IsCompleted)
            {
                throw new Exception("Error: attempted to rollback a transaction before it was processed.");
            }

            if(ResultTask.Result != CargoResult.Success)
            {
                return;//This state probably isn't valid, but just in case...
            }

            _cargoHandler.AddStatefulCargo(RemovedCargo, true);
            ResultCompletionSource.SetResult(CargoResult.RolledBack);            
        }

        public override void CloseTransaction()
        {
            if(OnCompletion != null)
            {
                OnCompletion(this, this);
            }
        }
    }


    /// <summary>
    /// Creates an ordered list of transactions which should be executed sequentially and atomically
    /// Sets result to true if completed succesfully, false if failed and rolled back.
    /// </summary>
    public class CargoTransactionSequence : ICargoTransaction, ITransactionSequence<CargoTransaction, ISyncerCargoHandler, CargoResult>
    {
        public CargoTransactionTypes TransactionType { get; protected set; }

        public Task<CargoResult> ResultTask { get; protected set; }
        public TaskCompletionSource<CargoResult> ResultCompletionSource { get; set; }
        public IReadOnlyList<CargoTransaction> Transactions { get { return _transactions; } }
        List<CargoTransaction> _transactions;

        public EventHandler<CargoTransactionSequence> OnCompletion;

        public CargoResult SequenceFailedValue { get { return CargoResult.SequenceFailed; } }
        public CargoResult IdLockAttemptTimeoutValue { get { return CargoResult.IDLockAttemptTimeout; } }
        public CargoResult SuccessValue { get { return CargoResult.Success; } }

        public CargoTransactionSequence()
        {
            ResultCompletionSource = new TaskCompletionSource<CargoResult>();
            ResultTask = ResultCompletionSource.Task;
            TransactionType = CargoTransactionTypes.TransactionSequence;
            _transactions = new List<CargoTransaction>();
        }
        
        public void Add(CargoTransaction t)
        {
            _transactions.Add(t);
            t.IsInSequence = true;
        }

        public void FinalizeTransaction()
        {
            foreach (CargoTransaction ct in Transactions)
            {
                ct.CloseTransaction();
            }
            if (OnCompletion != null)
            {
                OnCompletion(this, this);
            }
        }

    }
           
     /// <summary>
    /// Types which specify the transaction requested of CargoSynchronizer
    /// </summary>
    public enum CargoTransactionTypes
    {
        AddStatelessCargo,
        RemoveStatelessCargo,
        AddStatefulCargo,
        RemoveStatefulCargo,
        SwapStatefulCargo,

        TransactionSequence,

    }

    /// <summary>
    /// Interface allows TransactionSequence transactions and individual transactions to be processed in order.
    /// </summary>
    public interface ICargoTransaction
    {
        CargoTransactionTypes TransactionType { get; }

        //Allows a consumer to await the result, which is processed in another thread
        Task<CargoResult> ResultTask { get; }
    }

}
