using Freecon.Core.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Freecon.Core
{
    /// <summary>
    /// Not currently using this model. Lidgren handles firing when things are ready.
    /// If we find that there are bugs with that model, then we can transition to this.
    /// </summary>
    public class EventLoop
    {
        private const int ConcurrentIOTasksMax = 40;

        /// <summary>
        /// Tasks that are currently being executed live here.
        /// </summary>
        private List<Task<Tuple<Action<IWorkerResult>, IWorkerResult>>> _currentTasks;

        /// <summary>
        /// This holds a queue of async work that is used to feed the workers.
        /// </summary>
        private Queue<AsyncWork> _asyncWorkQueue;

        public EventLoop()
        {
            _currentTasks = new List<Task<Tuple<Action<IWorkerResult>, IWorkerResult>>>();
            _asyncWorkQueue = new Queue<AsyncWork>();
        }

        public async Task Loop(CancellationToken mainToken)
        {
            while (!mainToken.IsCancellationRequested)
            {
                // Async work
                while (_asyncWorkQueue.Count > 0 && _currentTasks.Count < ConcurrentIOTasksMax)
                {
                    var workContainer = _asyncWorkQueue.Dequeue();

                    // This pops an async work task off of the queue and executes it.
                    var w = Task<Tuple<Action<IWorkerResult>, IWorkerResult>>.Run(async () =>
                    {
                        var awaited = workContainer.WorkTask(workContainer.Token);

                        // This is a fancy way of handling timeouts easily.
                        // Essentially, we just wait to see if our task completes first
                        // Or if the Task.Delay fires first. We then check which task returned,
                        // Which allows us to determine what happened.
                        if (await Task.WhenAny(awaited, Task.Delay(workContainer.Timeout, workContainer.Token)) == awaited)
                        {
                            // Task completed within timeout.
                            // Consider that the task may have faulted or been canceled.
                            // We re-await the task so that any exceptions/cancellation is rethrown.
                            return Bundle(workContainer.Callback, await awaited);
                        }
                        else if (workContainer.Token.IsCancellationRequested)
                        {
                            // We asked to cancel the task and the task obeyed us.
                            return null;//Bundle(workContainer.Callback, new WorkerExceptionResult(new TaskCanceledException()));
                        }
                        else
                        {
                            // We timed out :/
                            return null;//Bundle(workContainer.Callback, new WorkerTimeoutResult());
                        }
                    });

                    _currentTasks.Add(w);
                }

                // Todo: Do syncronous updates (managers, etc)
                // Todo: Replace tuples with a real type.

                // Figure out which tasks are completed.
                var completedWork = new List<Task<Tuple<Action<IWorkerResult>, IWorkerResult>>>();
                foreach (var runningTask in _currentTasks)
                {
                    if (runningTask.IsCompleted)
                    {
                        completedWork.Add(runningTask);
                    }
                }

                // Remove the completed tasks and fire callbacks.
                foreach (var remove in completedWork)
                {
                    _currentTasks.Remove(remove);

                    var result = remove.Result;

                    // Callback to the original function.
                    result.Item1(result.Item2);
                }
            }
        }

        public void AddAsyncWork(AsyncWork work)
        {
            lock (_asyncWorkQueue)
            {
                _asyncWorkQueue.Enqueue(work);
            }
        }

        private Tuple<Action<IWorkerResult>, IWorkerResult> Bundle(Action<IWorkerResult> cb, IWorkerResult r)
        {
            return new Tuple<Action<IWorkerResult>, IWorkerResult>(cb, r);
        }
    }
}
