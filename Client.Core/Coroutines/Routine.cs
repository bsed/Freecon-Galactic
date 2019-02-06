using System.Collections.Generic;
using Core.Interfaces;
using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.Core.Coroutines
{
    public abstract class Routine : ISynchronousUpdate
    {
        /// <summary>
        /// Executes the routine
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Updates the routine
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(IGameTimeService gameTime);

        /// <summary>
        /// Gets if this coroutine has finished
        /// </summary>
        public bool Done { get; protected set; }
    }

    public class TaskStack
    {
        protected Stack<IEnumerable<ICoroutineResult>> _taskStack;

        public TaskStack()
        {
            _taskStack = new Stack<IEnumerable<ICoroutineResult>>();
        }

        public TaskStack(IEnumerable<ICoroutineResult> tasks) : this()
        {
            _taskStack.Push(tasks);
        }

        //public IEnumerable<ICoroutineResult> Run(IEnumerable<ICoroutineResult> nodes)
        //{
        //    var tasks = new Stack<ICoroutineResult>(nodes);

        //    while (tasks.Any())
        //    {
        //        var next = tasks.Pop();

        //        //yield return Run(next);

        //        //foreach (var child in next.Children)
        //        //{
        //        //    stack.Push(child);
        //        //}
        //    }
        //}
    }

    public class AIRoutine : Routine
    {
        public IEnumerable<ICoroutineResult> Tasks { get; protected set; }

        public AIRoutine(IEnumerable<ICoroutineResult> tasks)
        {
            Tasks = tasks;
        }

        public override void Execute()
        {
        }

        public override void Update(IGameTimeService gameTime)
        {


            //if (!_enumerator.MoveNext())
            //{
            //    Done = true;
            //}

            //var result = _enumerator.Current.ResultType;

            //switch (result)
            //{
            //    case CoroutineResultType.Wait:

            //    case CoroutineResultType.Done:
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
        }
    }
}