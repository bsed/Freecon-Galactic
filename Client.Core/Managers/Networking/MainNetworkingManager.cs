using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Core.Interfaces;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Core.States;
using Freecon.Client.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Core.Networking.Models;
using Freecon.Client.Core.Managers;
using Core.Utilities;
using Freecon.Core.Networking.Models.Messages;
using Core.Networking;
using Freecon.Core.Networking.Interfaces;
using Core.Networking.Interfaces;
using Server.Managers;

namespace Freecon.Client.Managers
{
    //public class NetworkMessageArrivedEventArgs : EventArgs
    //{
    //    public NetworkMessageContainer Message { get; set; }

    //    public MessageTypes Type { get; private set; }

    //    public NetworkMessageArrivedEventArgs(NetworkMessageContainer message, MessageTypes type)
    //    {
    //        Message = message;
    //        Type = type;
    //    }
    //}

    public class LidgrenNetworkingService:INetworkingService
    {
        List<NetworkMessageContainer> _pendingMessages = new List<NetworkMessageContainer>();

        event EventHandler<NetworkMessageContainer> _messageReceived;

        public void TriggerMessageReceived(NetworkMessageContainer message)
        {
            _pendingMessages.Add(message);
        }

        public void RegisterMessageHandler(INetworkMessageHandler handlerObject, EventHandler<NetworkMessageContainer> handlerMethod)
        {
            _messageReceived += handlerMethod;
        }

        public void DeregisterMessageHandler(INetworkMessageHandler handlerObject, EventHandler<NetworkMessageContainer> handlerMethod)
        {
            _messageReceived -= handlerMethod;
        }

        public void DeregisterAllHandlers(INetworkMessageHandler handlerObject)
        {
            if (_messageReceived != null)
            {
                foreach (var h in _messageReceived.GetInvocationList().Where(h => h.Target == handlerObject).Where(h => _messageReceived != null))
                {
                    _messageReceived -= (EventHandler<NetworkMessageContainer>) h;
                }
            }
        }

         /// <summary>
        /// Synchronously processes all pending messages for the corresponding areaID
        /// </summary>
        /// <param name="areaID"></param>
        public void FlushMessages(MessageHandlerID messageHandlerID)
        {
            foreach (var message in _pendingMessages)
            {
                _messageReceived(this, message);
            }
            _pendingMessages.Clear();
        }
        
    }
         
    public class MainNetworkingManager: ISynchronousManager
    {

        public static float LastLatency = 0;

        public bool SendPositionUpdates;

        //Hacky way to switch state before broadcasting NetworkingService.MessageReceived
        public bool ChangeGameState;
        public GameStateType NextState;

        NewChatManager _chatManager;
        ClientManager _clientManager;
        LidgrenNetworkingService _networkingService;

        public MainNetworkingManager(
                                     NewChatManager chatManager,
                                     ClientManager clientManager,
                                     LidgrenNetworkingService networkingService
                                     )
        {
            _chatManager = chatManager;

            _clientManager = clientManager;          

            _networkingService = networkingService;     
        }

        /// <summary>
        /// Networking update.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(IGameTimeService gameTime)
        {            
            NetIncomingMessage msg = null;

            if (_clientManager.Client == null)
                return;//This check shouldn't be necessary, something needs to be rewritten...

            while ((msg = _clientManager.Client.ReadMessage()) != null) // Set to While to catch multiple messages
            {
                if (msg.MessageType != NetIncomingMessageType.Data)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            if (msg.SenderConnection != _clientManager.CurrentMasterConnection && msg.SenderConnection.Status == NetConnectionStatus.Disconnected ||
                                msg.SenderConnection.Status == NetConnectionStatus.Disconnecting)
                            {
                                Console.WriteLine("Server status is timed out. Switching back to login state");
                                NextState = GameStateType.Login;
                                ChangeGameState = true;
                                return;
                                //_clientManager.Client.("Heartbeat Timed Out");
                                //_gameStateManager.SetState(GameStates.Disconnected);
                            }                            
                            break;


                        case NetIncomingMessageType.ConnectionLatencyUpdated:
                            LastLatency = msg.ReadFloat();
                            break;

                        default:
                            ConsoleManager.WriteLine("Non data message recieved: " + msg.ReadString(), ConsoleMessageType.NetworkMessage);
                            break;
                    }
                }
                else
                {
                    ProcessMessage(gameTime, msg);
                }

                if (msg != null)
                {
                    _clientManager.Client.Recycle(msg);
                }
            }

            if (_clientManager.CurrentSlaveConnection == null || _clientManager.CurrentSlaveConnection.Status != NetConnectionStatus.Connected)
                return;
            
            if (gameTime.TotalMilliseconds - _clientManager.LastSentServerTimeSync > 750 && SendPositionUpdates)
                _sendTimeSync(gameTime);

        }

        private void ProcessMessage(IGameTimeService gameTime, NetIncomingMessage msg)
        {
            var messageContainer = SerializationUtilities.DeserializeMsgPack<NetworkMessageContainer>(msg);

            if (messageContainer == null)
            {
                ConsoleManager.WriteLine("Corrupt message received", ConsoleMessageType.Warning);
                return;
            }

#if DEBUG
            if (messageContainer.MessageType != MessageTypes.PositionUpdateData
                && messageContainer.MessageType != MessageTypes.FireRequestResponse)
            {
                //ConsoleManager.WriteLine(messageContainer.MessageType + " Message Received", ConsoleMessageType.NetworkMessageType);
            }
#endif


            switch (messageContainer.MessageType) // Add new message types to this.
            {
                case MessageTypes.EnterColony:
                {
                    ChangeGameState = true;
                    NextState = GameStateType.Colony;

                    break;
                }
                case MessageTypes.PortDockApproval:
                {
                    ChangeGameState = true;
                    NextState = GameStateType.Port;
                    break;
                }

                case MessageTypes.PlanetLandApproval:
                {
                    ChangeGameState = true;
                    NextState = GameStateType.Planet;
                    break;
                }

                case MessageTypes.StarSystemData:
                {
                    ChangeGameState = true;
                    NextState = GameStateType.Space;
                    break;
                }

                case MessageTypes.WarpApproval:
                {
                    ChangeGameState = true;
                    NextState = GameStateType.Space;
                    break;
                }

                    #region Receive Port Ship

                case MessageTypes.ReceiveNewPortShip:
                {
                    break;
                }

                    #endregion

                    #region Receive Heartbeat

                case (MessageTypes.ServerHeartbeat):

                    _clientManager.LastSentServerTimeSync = gameTime.TotalMilliseconds;

                    break;

                    #endregion

                    #region Chat Message

                case (MessageTypes.ChatMessage):
                {
                    var data = messageContainer.MessageData as MessageChatMessage;

                    _chatManager.AddChat(data.ChatMessageData.ChatJson, data.ChatMessageData.MetaJson);

                    //Reads chat type and content, displays to chatbox
                    break;
                }

                    #endregion

                default:

                    //Console.WriteLine("Unsupported message type " + (MessageTypes)messageHeader + " received");
                    break;
            }

            _networkingService.TriggerMessageReceived(messageContainer);
        }

        private void _sendTimeSync(IGameTimeService gameTime)
        {
            var data = new MessageTimeSync();
            data.TimeMS = gameTime.TotalMilliseconds;

            NetOutgoingMessage outMessage = _clientManager.Client.CreateMessage();
            data.WriteToLidgrenMessage_ToServer(MessageTypes.TimeGet, outMessage);
            _clientManager.Client.SendMessage(outMessage, _clientManager.CurrentSlaveConnection, NetDeliveryMethod.UnreliableSequenced);

            _clientManager.LastSentServerTimeSync = gameTime.TotalMilliseconds;
        }

        public void Draw(Camera2D camera)
        { }
    }
}
