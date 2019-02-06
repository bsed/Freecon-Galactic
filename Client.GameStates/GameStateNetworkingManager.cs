using Freecon.Client.Core.States;
using Freecon.Core.Networking.Models;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Core.Interfaces;
using Core.Networking.Interfaces;
using Core.Networking;

namespace Freecon.Client.GameStates
{
    /// <summary>
    /// Currently Disabled, state changes happen in MainNetworkingManager
    /// </summary>
    public class GameStateNetworkingManager:INetworkMessageHandler
    {
        public MessageHandlerID MessageHandlerID { get { return _messageHandlerID; } }
        MessageHandlerID _messageHandlerID;


        IGameStateManager _gameStateManager;

        LidgrenNetworkingService _networkingService;

        MessageService_ToServer _messageManager;

        public bool IsUpdating { get; set; }


        public GameStateNetworkingManager(
            int managerID,
            IGameStateManager gameStateManager,
            LidgrenNetworkingService networkingService,
            MessageService_ToServer messageManager
            )
        {

            _gameStateManager = gameStateManager;
            _networkingService = networkingService;
            _messageManager = messageManager;
           
            _messageHandlerID = new MessageHandlerID(managerID);


            _networkingService.RegisterMessageHandler(this, _GameStateNetworkingManager_MessageReceived);
        }

        void _GameStateNetworkingManager_MessageReceived(object sender, NetworkMessageContainer e)
        {            
            switch (e.MessageType)
            {                                            

                case MessageTypes.EnterColony:
                {
                    _gameStateManager.SetState(GameStateType.Colony);
                    break;
                }
                case MessageTypes.PortDockApproval:
                {
                    _gameStateManager.SetState(GameStateType.Port);
                    break;
                }
                case MessageTypes.PlanetLandApproval:
                {
                    _gameStateManager.SetState(GameStateType.Planet);                   
                    break;
                }
                case MessageTypes.StarSystemData:
                {
                    _gameStateManager.SetState(GameStateType.Space);
                    break;
                }
                case MessageTypes.WarpApproval:
                {
                    _gameStateManager.SetState(GameStateType.Space);
                    break;

                }                
                
            }
        }
   
        ~GameStateNetworkingManager()
        {
            _networkingService.DeregisterMessageHandler(this, _GameStateNetworkingManager_MessageReceived);
        }
    }
}
