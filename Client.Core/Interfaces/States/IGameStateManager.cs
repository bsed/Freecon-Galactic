using Freecon.Client.Core.States;
using Freecon.Client.Core.States.Components;
using Core.Interfaces;

namespace Freecon.Client.Core.Interfaces
{
    public interface IGameStateManager : IDraw
    {
        IGameState ActiveState { get; }

        bool IsPlayableGameState { get; }

        void SetState(GameStateType type);

        void Update(IGameTimeService gameTime);
    }
}
