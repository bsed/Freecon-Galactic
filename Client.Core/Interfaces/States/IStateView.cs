using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.Core.States
{
    public interface IStateView : ISynchronousUpdate, IDraw
    {

    }

    public interface IStateView<TState> : IStateView
        where TState : IState
    {
        TState State { get; }
    }
}
