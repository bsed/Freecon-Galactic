using Freecon.Client.Core.States;
using Freecon.Client.Managers;

namespace Freecon.Client.Core.Services
{

    /// <summary>
    /// Inspects server messages and indirectly initiates state changes as appropriate.
    /// Must be checked by 
    /// </summary>
    public class StateChangeService
    {
        public bool SwitchToNextState { get; protected set; }
        public GameStateType NextState { get; protected set; }

        public StateChangeService(LidgrenNetworkingService networkingService)
        {


        }


    }
}
