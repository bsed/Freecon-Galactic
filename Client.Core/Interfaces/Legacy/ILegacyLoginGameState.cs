namespace Freecon.Client.Core.Interfaces.Legacy
{
    public interface ILegacyLoginGameState : ILegacyGameState
    {
        void Logout();

        void LoginComplete();
    }
}
