using System;

namespace Freecon.Core.Objects
{
    public interface IWorkerResult
    {
        /// <summary>
        /// Event that gets fired
        /// </summary>
        Action<IWorkerResult> Callback { get; }

        ResultType Type { get; }
    }

    public class WorkerCompletedResult: IWorkerResult
    {
        public Action<IWorkerResult> Callback { get; private set; }

        public IWorkerResult Result { get; private set; }

        public ResultType Type { get { return ResultType.WorkerCompleted; } }

        public WorkerCompletedResult(Action<IWorkerResult> cb, IWorkerResult result)
        {
            Callback = cb;
            Result = result;
        }
    }

    public class WorkerExceptionResult: IWorkerResult
    {
        public Action<IWorkerResult> Callback { get; private set; }

        public Exception CapturedException { get; private set; }

        public ResultType Type { get { return ResultType.WorkerException; } }

        public WorkerExceptionResult(Action<IWorkerResult> cb, Exception e)
        {
            Callback = cb;
            CapturedException = e;
        }
    }

    public class WorkerTimeoutResult: IWorkerResult
    {
        public Action<IWorkerResult> Callback { get; private set; }

        public ResultType Type { get { return ResultType.WorkerException; } }

        public WorkerTimeoutResult(Action<IWorkerResult> cb)
        {
            Callback = cb;
        }
    }

    public enum ResultType
    {
        WorkerCompleted,
        WorkerException,
        WorkerTimeout
    }
}
