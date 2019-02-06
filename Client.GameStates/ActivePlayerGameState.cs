using Freecon.Client.Managers;
using Core.Networking;
using Freecon.Client.Core.States;
using Freecon.Client.Core.States.Components;
using Freecon.Client.Managers.Networking;
using Freecon.Core.Networking.Interfaces;

namespace Freecon.Client.GameStates
{
    public abstract class ActivePlayerGameState : NetworkedGameState
    {
        protected PlayerShipManager _playerShipManager;

        public ActivePlayerGameState(MessageHandlerID messageHandlerId,
            IClientPlayerInfoManager clientPlayerInfoManager,
            INetworkingService networkingService,
            MessageService_ToServer messageService,
            PlayerShipManager playerShipManager,
            GameStateType stateType)
            : base(messageHandlerId, clientPlayerInfoManager, networkingService, messageService, stateType)
        {
            _playerShipManager = playerShipManager;

        }

        public override void Activate(IGameState previous)
        {
            //Debug
            Debugging.playerShipManager = _playerShipManager;

            base.Activate(previous);
            
        }
    }
}
