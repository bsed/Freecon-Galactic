
namespace Core.Interfaces
{
    public interface IGameTimeService
    {
        /// <summary>
        /// Total milliseconds that have elapsed since program start
        /// </summary>
        float TotalMilliseconds { get; }

        /// <summary>
        /// Milliseconds which have elapsed since the last global update
        /// </summary>
        float ElapsedMilliseconds { get; }


    }
}
