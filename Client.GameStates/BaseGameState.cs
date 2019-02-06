using Core.Interfaces;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.States;
using Freecon.Client.Core.States.Components;
using System.Collections.Generic;
using Freecon.Client.View.CefSharp.States;

namespace Freecon.Client.GameStates
{
    public abstract class BaseGameState : IGameState
    {
        

        public GameStateStatus Status { get; protected set; }

        protected HashSet<ISynchronousUpdate> _synchronousUpdateList { get; set; }
        /// <summary>
        /// Updates first. Updates for the game state go here.
        /// </summary>
        public virtual HashSet<ISynchronousUpdate> SynchronousUpdateList
        {
            get { return _synchronousUpdateList; }
        }

        protected HashSet<ISynchronousUpdate> _viewUpdateList { get; set; }
        /// <summary>
        /// Updates after the core game update has completed.
        /// </summary>
        public virtual HashSet<ISynchronousUpdate> ViewUpdateList
        {
            get { return _viewUpdateList; }
        }

        protected HashSet<IAsynchronousUpdate> _asynchronousUpdateList { get; set; }
        /// <summary>
        /// In theory, should be used for CPU-intensive tasks that need to be completed in other threads
        /// and span multiple update calls.
        /// </summary>
        public virtual HashSet<IAsynchronousUpdate> AsynchronousUpdateList
        {
            get { return _asynchronousUpdateList; }
        }

        public bool IsUpdating { get; set; }

        private GameStateType _stateType;
        public GameStateType StateType
        {
            get { return _stateType; }
        }

        public BaseGameState(GameStateType stateType)
        {
            _synchronousUpdateList = new HashSet<ISynchronousUpdate>();
            _viewUpdateList = new HashSet<ISynchronousUpdate>();
            _asynchronousUpdateList = new HashSet<IAsynchronousUpdate>();
            _stateType = stateType;
        }

        public virtual void Activate(IGameState previous)
        {
            Status = GameStateStatus.Active;
        }

        public virtual void Deactivate(IGameState next)
        {
            Clear();
            Status = GameStateStatus.Inactive;
        }

        /// <summary>
        /// Resets the game state, clearing all stateful data, to be called on deactivation
        /// </summary>
        public virtual void Clear()
        {
           
        }

        public virtual void StateWillUpdate(IGameTimeService gameTime)
        {
            IsUpdating = true;
        }

        public virtual void StateDidUpdate(IGameTimeService gameTime)
        {
            IsUpdating = false;
        }

        public virtual UIStateManagerContainer GetStateManagerContainer()
        {
            return new UIStateManagerContainer(StateType);
        }
        
    }
}
