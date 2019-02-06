using Freecon.Core.Objects;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace Freecon.Core.Utilities
{
    public class DeadlockTimer : System.Timers.Timer
    {
        public Thread ThreadToMonitor { get; protected set; }

        public DeadlockTimer(Thread threadToMonitor, float timeout)
            : base(timeout)
        {
            ThreadToMonitor = threadToMonitor;
            AutoReset = false;
            Elapsed += DeadlockTimer_Elapsed;
        }

        void DeadlockTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var ex = new DeadlockDetectedException();
            ex.StackTrace = GetStackTrace(ThreadToMonitor);
        }



        /// <summary>
        /// Fancy stack tracer I stole from stackoverflow
        /// Credit to user Joe Albahari
        /// </summary>
        /// <param name="targetThread"></param>
        /// <returns></returns>
        StackTrace GetStackTrace(Thread targetThread)
        {
            StackTrace stackTrace = null;
            var ready = new ManualResetEventSlim();

            new Thread(() =>
            {
                // Backstop to release thread in case of deadlock:
                ready.Set();
                Thread.Sleep(200);
                try { targetThread.Resume(); }
                catch { }
            }).Start();

            ready.Wait();
            targetThread.Suspend();
            try { stackTrace = new StackTrace(targetThread, true); }
            catch { /* Deadlock */ }
            finally
            {
                try { targetThread.Resume(); }
                catch { stackTrace = null;  /* Deadlock */  }
            }

            return stackTrace;
        }

    }




    


}
