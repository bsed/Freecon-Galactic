using Freecon.Client.Core.States;
using Freecon.Core.Networking.Models;
using Freecon.Client.Managers;
using Freecon.Core.Networking.Interfaces;
using Core.Networking;
using Core.Networking.Interfaces;
using Core.Interfaces;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Managers.Networking;

namespace Freecon.Client.GameStates
{
    public abstract class NetworkedGameState : BaseGameState, INetworkMessageHandler, ISynchronousUpdate
    {
        public MessageHandlerID MessageHandlerID { get { return _messageHandlerID; } }
        readonly MessageHandlerID _messageHandlerID;

        protected readonly IClientPlayerInfoManager _clientPlayerInfoManager;

        protected INetworkingService _networkingService;
        protected MessageService_ToServer _MessageService;

        public NetworkedGameState( MessageHandlerID messageHandlerId,
            IClientPlayerInfoManager playerInfoManager,
            INetworkingService networkingService,
            MessageService_ToServer messageService,
            GameStateType stateType)
            : base(stateType)
        {
            _clientPlayerInfoManager = playerInfoManager;
            _messageHandlerID = messageHandlerId;
            _networkingService = networkingService;
            _synchronousUpdateList.Add(this);
            _networkingService.RegisterMessageHandler(this, _networkedGameState_MessageReceived);
            _MessageService = messageService;
        }

        void _networkedGameState_MessageReceived(object sender, NetworkMessageContainer e)
        {
           
        }

        public void Update(IGameTimeService gameTime)
        {
            _networkingService.FlushMessages(_messageHandlerID);

        }

        public virtual void Dispose()
        {
            _networkingService.DeregisterAllHandlers(this);
        }

        

        ~NetworkedGameState()
        {
            
        }
               
    }
}
