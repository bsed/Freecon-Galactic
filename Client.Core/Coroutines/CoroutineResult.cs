namespace Freecon.Client.Core.Coroutines
{
    public abstract class CoroutineResult : ICoroutineResult
    {
        public CoroutineResultType ResultType { get; protected set; }
    }

    public class DoneResult : CoroutineResult
    {
        public DoneResult()
        {
            ResultType = CoroutineResultType.Done;
        }
    }

    public class WaitResult : CoroutineResult
    {
        public WaitResult()
        {
            ResultType = CoroutineResultType.Wait;
        }
    }
}