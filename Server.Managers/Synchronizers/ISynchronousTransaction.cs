using Server.Models.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Managers.Synchronizers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="SyncherVMType">The syncher view model type used to modify the underlying synchronized model</typeparam>
    /// <typeparam name="ResultType"></typeparam>
    public interface ISynchronousTransaction<SyncherVMType, ResultType>
    {
        /// <summary>
        /// A unique identifier which a Synchronizer can use to lock an object
        /// </summary>
        object UniqueLockableId { get; }

        //Allows a consumer to await the result, which is processed in another thread
        Task<ResultType> ResultTask { get; }
        TaskCompletionSource<ResultType> ResultCompletionSource { get; set; }

        ResultType Process();

        void Rollback();

        IHasCargo CargoHolder { get; }

        /// <summary>
        /// Allows the synchronizer to resolve the correct syncher vm type, abstracting the logic away from the transaction scheduler
        /// </summary>
        /// <param name="vm"></param>
        void SetSyncherVM(SyncherVMType vm);

        /// <summary>
        /// Returns the ResultType value equal to success; needed because of generics
        /// </summary>
        /// <returns></returns>
        ResultType SuccessValue { get; }

        /// <summary>
        /// Sets the status to fail
        /// </summary>
        ResultType IdLockAttemptTimeoutValue { get; }
                
        /// <summary>
        /// Transaction is a part of a sequence
        /// </summary>
        bool IsInSequence { get; }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="SynchronousTransactionType"></typeparam>
    /// <typeparam name="SyncherVMType">The syncher view model type used to modify the underlying synchronized model</typeparam>
    /// <typeparam name="ResultType"></typeparam>
    public interface ITransactionSequence<SynchronousTransactionType, SyncherVMType, ResultType>
        where SynchronousTransactionType:ISynchronousTransaction<SyncherVMType, ResultType>
    {

        /// <summary>
        /// Returns value equal to ResultType.SequenceFailed
        /// </summary>
        /// <returns></returns>
        ResultType SequenceFailedValue { get; }

        /// <summary>
        /// Returns value equal to ResultType.IDLockAttemptTimeout
        /// </summary>
        /// <returns></returns>
        ResultType IdLockAttemptTimeoutValue { get; }

        /// <summary>
        /// Returns the ResultType value equal to success; needed because of generics
        /// </summary>
        /// <returns></returns>
        ResultType SuccessValue { get; }

        //Allows a consumer to await the result, which is processed in another thread
        Task<ResultType> ResultTask { get; }
        TaskCompletionSource<ResultType> ResultCompletionSource { get; set; }

        IReadOnlyList<SynchronousTransactionType> Transactions { get; }

        void Add(SynchronousTransactionType transaction);

        void FinalizeTransaction();

        
    }

    public interface ITransferDonor
    {
        bool TransferToNextTransaction { get; }

        object TransferData { get; }
    }

    public interface ITransferRecipient
    {
        object TransferData { get; set; }
    }
}
