using Freecon.Client.Core.Interfaces;
using Core.Interfaces;
using System.Collections.Generic;
using Freecon.Client.View.CefSharp.States;

namespace Freecon.Client.Core.States.Components
{
    public interface IGameState
    {
        GameStateStatus Status { get; }

        GameStateType StateType { get; }

        HashSet<IAsynchronousUpdate> AsynchronousUpdateList { get; }

        HashSet<ISynchronousUpdate> SynchronousUpdateList { get; }

        HashSet<ISynchronousUpdate> ViewUpdateList { get; }

        void Activate(IGameState previous);

        void Deactivate(IGameState next);

        /// <summary>
        /// Logic to be performed before UpdateList is iterated over.
        /// </summary>
        /// <param name="gameTime">Standard IGameTimeService</param>
        void StateWillUpdate(IGameTimeService gameTime);

        /// <summary>
        /// Logic to be performed after UpdateList is iterated over.
        /// </summary>
        /// <param name="gameTime">Standard IGameTimeService</param>
        void StateDidUpdate(IGameTimeService gameTime);

        /// <summary>
        /// Returns a IGameState to its initialization state.
        /// </summary>
        void Clear();

        UIStateManagerContainer GetStateManagerContainer();

    }
}
