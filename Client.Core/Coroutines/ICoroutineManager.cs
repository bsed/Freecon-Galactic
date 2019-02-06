using System.Collections;
using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.Core.Coroutines
{
    /// <summary>
    /// Executes and manages coroutines
    /// </summary>
    public interface ICoroutineManager : ISynchronousUpdate
    {
        void Run(IEnumerable source);
    }
}