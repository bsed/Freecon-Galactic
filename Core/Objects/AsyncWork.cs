using System;
using System.Threading;
using System.Threading.Tasks;

namespace Freecon.Core.Objects
{
    /// <summary>
    /// Represents an asyncronous work task.
    /// </summary>
    public class AsyncWork
    {
        public Action<IWorkerResult> Callback { get; private set; }

        public DateTime EndTime { get; private set; }

        public DateTime StartTime { get; private set; }

        public int Timeout { get; private set; }

        public CancellationToken Token { get; private set; }

        public Func<CancellationToken, Task<IWorkerResult>> WorkTask { get; private set; }

        /// <summary>
        /// Creates some work to be executed asyncronously.
        /// Assumes a timeout of 3000ms to be completed once started.
        /// </summary>
        /// <param name="work">The work to be executed asyncronously.
        /// This is any function with that looks like...
        /// <code>Task<IWorkerResult> MyFunction(CancellationToken)</code>
        /// or
        /// <code>async IWorkerResult MyFunction(CancellationToken)</code></param>
        /// <param name="token">
        /// A cancellation token that can be invoked to attempt to cancel the task
        /// once started. Make sure your work can gracefully degrade.
        /// </param>
        public AsyncWork(Func<CancellationToken, Task<IWorkerResult>> work, CancellationToken token, Action<IWorkerResult> callback)
        {
            SetWork(work);
            Token = token;
            Timeout = 3000;
            Callback = callback;
        }

        /// <summary>
        /// Creates some work to be executed asyncronously.
        /// Assumes a timeout of 3000ms to be completed once started.
        /// </summary>
        /// <param name="work">The work to be executed asyncronously.
        /// This is any function with that looks like...
        /// <code>Task<IWorkerResult> MyFunction(CancellationToken)</code>
        /// or
        /// <code>async IWorkerResult MyFunction(CancellationToken)</code></param>
        /// <param name="token">
        /// A cancellation token that can be invoked to attempt to cancel the task
        /// once started. Make sure your work can gracefully degrade.
        /// </param>
        public AsyncWork(Func<CancellationToken, Task<IWorkerResult>> work, CancellationToken token, Action<IWorkerResult> callback, int timeout)
        {
            SetWork(work);
            Token = token;
            Timeout = timeout;
            Callback = callback;
        }

        /// <summary>
        /// Wraps the function call with some meta data about the task's execution.
        /// </summary>
        /// <param name="work"></param>
        private void SetWork(Func<CancellationToken, Task<IWorkerResult>> work)
        {
            WorkTask = async (ct) =>
            {
                StartTime = DateTime.Now;

                // If we want to cancel a task before starting...
                // All tasks will obey this.
                ct.ThrowIfCancellationRequested();

                var result = await work(ct);
                EndTime = DateTime.Now;
                return result;
            };
        }
    }
}
