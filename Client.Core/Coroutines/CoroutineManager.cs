using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Freecon.Core.Utils;

namespace Freecon.Client.Core.Coroutines
{
    /// <summary>
    /// Manages active co routines
    /// </summary>
    public class CoroutineManager : ICoroutineManager
    {
        private readonly IList<RoutineHandle> _routines;

        /// <summary>
        /// Initializes a new instance of <see cref="CoroutineManager"/>
        /// </summary>
        public CoroutineManager()
        {
            _routines = new List<RoutineHandle>();
        }

        public void Run(IEnumerable source)
        {
            var handler = new RoutineHandle(source);
            _routines.Add(handler);
            handler.Step();
        }

        /// <summary>
        /// Updates this <see cref="GameComponent"/>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(IGameTimeService gameTime)
        {
            _routines.ForEach(routineHandle => routineHandle.Update(gameTime));

            foreach (var handle in _routines.Where(handle => handle.Done).ToList())
            {
                _routines.Remove(handle);
            }
        }
    }
}