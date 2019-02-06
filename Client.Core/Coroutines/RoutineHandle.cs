using System.Collections;
using Core.Interfaces;

namespace Freecon.Client.Core.Coroutines
{
    internal class RoutineHandle
    {
        private readonly IEnumerator _routines;

        public bool Done
        {
            get { return _routines.Current == null; }
        }

        public RoutineHandle(IEnumerable routines)
        {
            _routines = routines.GetEnumerator();
        }

        public void Update(IGameTimeService gameTime)
        {

            // Maybe do some nifty type detection here 
            // float values are total seconds
            // bool value of false should halt coroutine execution
            // int value skips x number of frames
            var routine = _routines.Current as Routine;

            if (routine == null || routine.Done)
            {
                Step();
            }
            else
            {
                routine.Update(gameTime);
            }
        }

        public void Step()
        {
            if (!_routines.MoveNext())
            {
                return;
            }

            var routine = _routines.Current as Routine;

            if (routine != null)
            {
                routine.Execute();
            }
        }

    }
}