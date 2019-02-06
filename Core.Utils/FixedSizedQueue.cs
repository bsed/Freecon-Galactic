using System.Collections.Generic;

namespace Freecon.Core.Utils
{
    public class FixedSizedQueue<T> : Queue<T>
    {
        public int MaxQueueSize { get; protected set; }
        private readonly object _syncRoot = new object();

        public FixedSizedQueue(int maxQueueSize)
        {
            MaxQueueSize = maxQueueSize;
        }

        public new void Enqueue(T item)
        {
            lock (_syncRoot)
            {
                base.Enqueue(item);

                if (Count > MaxQueueSize)
                {
                    // Throw away
                    Dequeue();
                }
            }
        }
    }
}
