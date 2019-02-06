using Core.Interfaces;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;

namespace Freecon.Client.Core.States
{
    public interface IStateStack : IDraw, ISynchronousUpdate
    {
    }

    public class StateStack<TState, TStateView> : IStateStack
        where TState : IState
        where TStateView : IStateView<TState>
    {
        public TState State { get; private set; }

        public TStateView StateView { get; private set; }

        public StateStack(TState state, TStateView stateView)
        {
            State = state;
            StateView = stateView;
        }

        public void Draw(Camera2D camera)
        {
            StateView.Draw(camera);
        }

        public void Update(IGameTimeService gameTime)
        {
            State.Update(gameTime);
            StateView.Update(gameTime);
        }
    }
}
