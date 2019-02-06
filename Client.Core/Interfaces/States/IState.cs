using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.Core.States
{
    public interface IState : ISynchronousUpdate
    {
        GameStateType StateType { get; }
    }
}
