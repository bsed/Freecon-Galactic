using Core.Models.Enums;

namespace Freecon.Client.Core.Interfaces.Legacy
{
    public interface ILegacyGameStateManager2
    {
        ILegacyGameState FetchGameState(GameStates state);

        bool getIfNewGameState();

        GameStates getState();

        void SetState(GameStates state);
    }
}
