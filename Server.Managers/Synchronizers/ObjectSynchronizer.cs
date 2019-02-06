using Core;
using Freecon.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Server.Managers.Synchronizers
{
    public abstract class Synchronizer<TransactionType, TransactionSequenceType, SyncherVMType, ResultType>:ISynchronizer
       where TransactionType:ISynchronousTransaction<SyncherVMType, ResultType>
       where TransactionSequenceType:ITransactionSequence<TransactionType, SyncherVMType, ResultType>
   {
        
        /// <summary>
        /// Unique IDs which are awaiting processing, to allow for safe concurrency. Value is timestamp at locking
        /// </summary>
        ConcurrentDictionary<object, float> _lockedIDs;

        /// <summary>
        /// Tokens for IDs which are locked as a part of transactions, to prevent deadlocks from occuring during processing of a given transaction
        /// </summary>
        ConcurrentDictionary<object, LockToken> _lockTokens;

        /// <summary>
        /// ms, time before automatically unlocking Id, just in case unlocking fails. Should be long enough for the largest of transaction sequences to complete
        /// </summary>
        protected float _lockTimeout = 1000;

        protected float _defaultLockAttemptTimeout = 20000;

        HashSet<int> _IDsToDeregister;

        object _updateLock = new object();

        List<System.Timers.Timer> _updateTimers;
        bool _isUpdating;

        ConcurrentQueue<TransactionType> _pendingTransactions;
        ConcurrentQueue<TransactionSequenceType> _pendingTransactionSequences;

        public Synchronizer()
        {
            _lockedIDs = new ConcurrentDictionary<object, float>();
            _lockTokens = new ConcurrentDictionary<object, LockToken>();
            _pendingTransactions = new ConcurrentQueue<TransactionType>();
            _IDsToDeregister = new HashSet<int>();
            _pendingTransactionSequences = new ConcurrentQueue<TransactionSequenceType>();
            _updateTimers = new List<System.Timers.Timer>();
        }

        /// <summary>
        /// Spawns a thread to periodically process transactions
        /// </summary>
        public void Start(float updatePeriod, int numThreads)
        {
            _isUpdating = true;
            _updateTimers.Clear();
            for(int i = 0; i < numThreads; i++)
            {
                var timer = new System.Timers.Timer(updatePeriod);
                timer.AutoReset = false;
                timer.Elapsed += Update;
                timer.Start();
                _updateTimers.Add(timer);
            }

            
            
        }

        public void Stop()
        {
            foreach (var t in _updateTimers)
            {
                t.Stop();
            } 
            _isUpdating = false;
        }

        void Update(object sender, EventArgs args)
        {
            if (!_isUpdating)
            {
                ((System.Timers.Timer)sender).Stop();
                return;
            }
            try
            {   
                while (_pendingTransactions.Count != 0)
                {
                    TransactionType t;
                    if (_pendingTransactions.TryDequeue(out t))//TryDequeue guarantees that an element can only be removed once
                    {
                        _processTransaction(t, _defaultLockAttemptTimeout);
                    }
                }

                while (_pendingTransactionSequences.Count != 0)
                {
                    TransactionSequenceType t;

                    if (_pendingTransactionSequences.TryDequeue(out t))//TryDequeue guarantees that an element can only be removed once
                    {
                        _processTransactionSequence(t, _defaultLockAttemptTimeout);

                    }

                }

                ((System.Timers.Timer)sender).Start();
                
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLine(e.Message, ConsoleMessageType.Error);
                ConsoleManager.WriteLine(e.StackTrace, ConsoleMessageType.Error);
                ((System.Timers.Timer)sender).Start();
                throw e;
            }
        }
       
        public void RequestTransaction(TransactionType transaction)
        {
            SetViewModel(transaction);
            _pendingTransactions.Enqueue(transaction);

        }

        /// <summary>
        /// The transactions are guaranteed to be executed atomically and sequentially, or not at all 
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns>seqence.Result</returns>
        public void RequestAtomicTransactionSequence(TransactionSequenceType sequence)
        {
            foreach(var t in sequence.Transactions)
            {
                SetViewModel(t);
            }

            _pendingTransactionSequences.Enqueue(sequence);            
        }

        protected abstract void SetViewModel(ISynchronousTransaction<SyncherVMType, ResultType> transaction);

        #region Processing

        /// <summary>
        /// Attempts to lock the subject of the transaction and returns true if the transaction is succesful, false otherwise
        /// </summary>
        /// <param name="c"></param>
        /// <param name="timeout">ms</param>
        /// <returns></returns>
        void _processTransaction(TransactionType c, float timeout)
        {
            
            if (!_lockModelId(c.UniqueLockableId, timeout, null))
            {
                c.ResultCompletionSource.SetResult(c.IdLockAttemptTimeoutValue);
                _unlockModelId(c.UniqueLockableId);
                return;
            }

            c.Process();

            _unlockModelId(c.UniqueLockableId);
        }

        /// <summary>
        /// Timeout applies to each individual transaction
        /// </summary>
        /// <param name="s"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        void _processTransactionSequence(TransactionSequenceType s, float timeout)
        {
            //Lock all IDs
            HashSet<object> idsToLock = new HashSet<object>(s.Transactions.Select(ss => ss.UniqueLockableId));
            HashSet<object> lockedIDs = new HashSet<object>();

            object lockToken = TimeKeeper.MsSinceInitialization;

            foreach(var id in idsToLock)
            {
                if(!_lockModelId(id, timeout, lockToken))
                {
                    _rollbackTransactionSequence(s, lockedIDs, -1, s.IdLockAttemptTimeoutValue);
                    return;
                }
                else
                {
                    lockedIDs.Add(id);
                }
            }
                                    

            //At this point, all IDs are locked
            int lastCompletedIndex = -1;
            for (int i = 0; i < s.Transactions.Count; i++)
            {

                if (!EqualityComparer<ResultType>.Default.Equals(s.Transactions[i].Process(),s.Transactions[i].SuccessValue))//If transaction fails, rollback
                {
                    _rollbackTransactionSequence(s, lockedIDs, lastCompletedIndex, s.Transactions[i].ResultTask.Result);

                    return;
                }
                else
                {
                    if (IsValidTransactionTransfer(s, i))
                    {
                        ((ITransferRecipient)s.Transactions[i + 1]).TransferData = ((ITransferDonor)s.Transactions[i]).TransferData;

                    }

                    lastCompletedIndex++;
                }

            }

            //At this point all transactions were succesfully processed.
            s.FinalizeTransaction();                            
            //Unlock IDs
            foreach (int i in lockedIDs)
            {
                if(!_unlockModelId(i))
                {
                    throw new CorruptStateException("Failed to unlock Id.");
                }
            }

            if(!_removeLockToken(lockToken))
            {
                throw new CorruptStateException("Failed to remove lock token.");
            }

            s.ResultCompletionSource.SetResult(s.SuccessValue);

        }

        #endregion

       /// <summary>
       /// Checks if the current and next transaction in the sequence will be transfering data from one to the other
       /// </summary>
       /// <param name="s"></param>
       /// <param name="donorTransaction"></param>
       /// <returns></returns>
        bool IsValidTransactionTransfer(TransactionSequenceType s, int donorTransactionIndex)
        {
            return 
                s.Transactions[donorTransactionIndex] is ITransferDonor && 
                ((ITransferDonor)s.Transactions[donorTransactionIndex]).TransferToNextTransaction && 
                s.Transactions.Count > donorTransactionIndex + 1 && 
                s.Transactions[donorTransactionIndex + 1] is ITransferRecipient;
        }

        void _rollbackTransactionSequence(TransactionSequenceType s, HashSet<object> lockedIDs, int lastCompletedIndex, ResultType failureReason)
        {
            ResultType sequenceFailed = s.SequenceFailedValue;

            //Rollback any transactions that were completed
            for (int i = lastCompletedIndex; i >= 0; i++)
            {
                s.Transactions[i].Rollback();
            }

            //Set any unprocessed transactions to fail condition
            for (int i = lastCompletedIndex + 1; i < s.Transactions.Count; i++)
            {
                s.Transactions[i].ResultCompletionSource.SetResult(sequenceFailed);
            }

            //Unlock IDs
            foreach (int i in lockedIDs)
            {
                _unlockModelId(i);
            }

            s.ResultCompletionSource.SetResult(failureReason);

        }
       
       /// <summary>
       /// If the id is already locked and the token is already in use, this method returns true
       /// </summary>
       /// <param name="id"></param>
       /// <param name="waitStartTime"></param>
       /// <param name="attemptTimeout"></param>
       /// <param name="token"></param>
       /// <returns></returns>
        bool _tryLock(object id, float waitStartTime, float attemptTimeout, object token)
        {
            LockToken lockToken = new LockToken(token, TimeKeeper.MsSinceInitialization);

             while(TimeKeeper.MsSinceInitialization - waitStartTime < attemptTimeout)
             {  
                    if(_lockedIDs.ContainsKey(id) && _lockTokens.ContainsKey(token))
                    {
                        return true;
                    }

                    if(!_lockedIDs.TryAdd(id, TimeKeeper.MsSinceInitialization))
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    
                                     
                    lockToken.LastUpdateTime = TimeKeeper.MsSinceInitialization;
                    if(!_lockTokens.ContainsKey(token) && !_lockTokens.TryAdd(token, lockToken))
                    {
                        throw new CorruptStateException("Succesfully locked an Id, but could not add the LockToken. This shouldn't be possible, probable concurrency bug.");
                    }

                    return true;
             }

            return false;
        }   

        bool _tryLock(object id, float waitStartTime, float attemptTimeout)
        {
            while(TimeKeeper.MsSinceInitialization - waitStartTime < attemptTimeout)
            {               
                if (_lockedIDs.TryAdd(id, TimeKeeper.MsSinceInitialization))
                {
                    return true;
                }
                Thread.Sleep(1);
            }

            return false;
        }

       /// <summary>
       /// Set token to null to lock a single transaction. Pass the same token object for each lock belonging to a single transaction. If an Id is already locked and a lock token exists
       /// </summary>
       /// <param name="id"></param>
       /// <param name="timeout"></param>
       /// <param name="token"></param>
       /// <returns></returns>
        bool _lockModelId(object id, float timeout, object token)
        {
            float lockTime;

            if(_lockedIDs.TryGetValue(id, out lockTime) && token != null && _lockTokens.ContainsKey(token))//Object is already locked, but the token indicates that this is part of a sequence
            {
                _lockTokens[token].LastUpdateTime = TimeKeeper.MsSinceInitialization;
                return true;
            }

            if (_lockedIDs.TryGetValue(id, out lockTime) && TimeKeeper.MsSinceInitialization - lockTime < _lockTimeout)//If the object is already locked, try to wait until it becomes available
            {
                float waitStartTime = TimeKeeper.MsSinceInitialization;

                bool lockSuccess = token==null?_tryLock(id, waitStartTime, _defaultLockAttemptTimeout):_tryLock(id, waitStartTime, _defaultLockAttemptTimeout, token);
                
                
            }
            else if (_lockedIDs.TryGetValue(id, out lockTime) && TimeKeeper.MsSinceInitialization - lockTime > _lockTimeout)//The id is locked, but the timeout is expired; 
            {
                throw new CorruptStateException("Locked id lock timeout expired, but the id was never removed.");
            }
            else
            {
                float waitStartTime = TimeKeeper.MsSinceInitialization;
                return token == null ? _tryLock(id, waitStartTime, _defaultLockAttemptTimeout) : _tryLock(id, waitStartTime, _defaultLockAttemptTimeout, token);
                
            }

            return false;
            
        }


        /// <summary>
        /// Returns true if succesful.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool _unlockModelId(object id)
        {
            float outv;
            return _lockedIDs.TryRemove(id, out outv);

        }

        bool _removeLockToken(object token)
        {
            LockToken outv;
            return _lockTokens.TryRemove(token, out outv);
        }

        class LockToken
        {
            public float LastUpdateTime;
                       
            public readonly object Token;

            public LockToken(object token, float lockTime)
            {
                Token = token;
                LastUpdateTime = lockTime;
            }
        }
             
   }
}
