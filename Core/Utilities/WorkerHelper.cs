using System.Threading;

namespace Freecon.Core.Utilities
{
    public static class WorkerHelper
    {
        public static CancellationToken CreateToken()
        {
            var ts = new CancellationTokenSource();
            return ts.Token;
        }
    }
}
