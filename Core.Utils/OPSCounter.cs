using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Core.Utils
{
    /// <summary>
    /// Simple class designed to count operations per second.
    /// </summary>
    public class OPSCounter
    {
        FixedSizedQueue<float> _periodBuffer;

        private readonly Stopwatch _stopWatch;

        private float _lastUpdateTicks;

        private readonly bool _useLocks;
        private readonly object _lockObject = new object();


        /// <summary>
        /// Initializes OPSCounter to work with the provided stopwatch
        /// </summary>
        /// <param name="bufferLength">Number of updates to smooth over</param>
        /// <param name="stopwatch"></param>
        /// /// <param name="useLocks">If true, locks on updates for thread safety</param>
        public OPSCounter(int bufferLength, bool useLocks = false)
        {
            _useLocks = useLocks;
            InitBuffer(bufferLength);
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            
        }

        /// <summary>
        /// Calculates the time elapsed since last update and adds the value to the buffer
        /// </summary>
        public void Update()
        {
            if (_useLocks)
            {
                lock (_lockObject)
                {
                    _update();
                }
            }
            else
            {
                _update();
            }
        }

        void _update()
        {
            _periodBuffer.Enqueue(_stopWatch.ElapsedTicks - _lastUpdateTicks);
            _lastUpdateTicks = _stopWatch.ElapsedTicks;
        }
        
        /// <summary>
        /// Returns the current number of operations per second, smoothed over the buffer
        /// </summary>
        /// <returns></returns>
        public float GetAverageOPS()
        {
            return (_periodBuffer.Count/_periodBuffer.Sum()) * Stopwatch.Frequency;
        }

        /// <summary>
        /// Returns the latest OPS value enqueued in the buffer, instead of smoothing over the whole buffer 
        /// </summary>
        /// <returns></returns>
        public float GetInstantaneousOPS()
        {
            return 1/(_periodBuffer.Last() / Stopwatch.Frequency * 1000f);
        }

        void InitBuffer(int length)
        {
            _periodBuffer = new FixedSizedQueue<float>(length);
            for (int i = 0; i < length; i++)
            {
                _periodBuffer.Enqueue(0);
            }
        }
        
    }
}
