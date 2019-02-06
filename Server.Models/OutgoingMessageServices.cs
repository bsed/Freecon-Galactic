using Core.Models.Enums;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Objects;
using Lidgren.Network;
using RedisWrapper;
using Server.Models.Interfaces;

namespace Server.Models
{
    /// <summary>
    /// Object which creates outgoing messages for lidgren
    /// </summary>
    public interface IMessageCreatorService
    {
        NetOutgoingMessage CreateMessage();
    }


    public class LidgrenOutgoingMessageService : IOutgoingMessageService
    {
        /// <summary>
        /// Message only needs to be generated on initialization and is reused.
        /// </summary>
        private IMessageCreatorService _messageCreatorService;

        public NetConnection Connection { get; protected set; }

        public LidgrenOutgoingMessageService(NetConnection connection, IMessageCreatorService messageCreatorService)
        {
            Connection = connection;
            _messageCreatorService = messageCreatorService;
        }

        public void SendMessage(NetworkMessageContainer message)
        {
            var msg = _messageCreatorService.CreateMessage();
            message.WriteToLidgrenMessage_ToClient(message.MessageType, msg);
            Connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered, 0);//TODO: implement probably static method to get sequence channel and delivery method from message type
        }

    }

    public class RedisOutgoingMessageService : IOutgoingMessageService
    {
        private NPCPlayer _player;
        private RedisServer _redisServer;

        public RedisOutgoingMessageService(RedisServer redisServer, NPCPlayer player)
        {
            _player = player;
            _redisServer = redisServer;
        }

        public void SendMessage(NetworkMessageContainer message)
        {
            var msg = new SimulatorBoundMessage(message.MessageData, message.MessageType, _player.CurrentAreaID.Value);
            _redisServer.PublishObject(ChannelTypes.ServerToSimulator_Data, _player.CurrentAreaID.Value, msg);

            
        }
    }
}
