using Core.Models.Enums;
using Freecon.Core.Networking.Objects;
using Freecon.Core.Utils;
using Lidgren.Network;
using MsgPack.Serialization;
using Newtonsoft.Json;

namespace Freecon.Core.Networking.Models
{
    public class NetworkMessageContainer : MessagePackSerializableObject
    {
        public MessageTypes MessageType { get { return _messageType; } set { _messageType = value; _messageTypeSet = true; } }
        MessageTypes _messageType;
        bool _messageTypeSet;

        /// <summary>
        /// Not required for client-server or server-client messages
        /// </summary>
        public RoutingData RoutingData { get; set; }

        [MessagePackRuntimeType]
        [JsonProperty]
        public MessagePackSerializableObject MessageData { get; set; }

        public NetworkMessageContainer()
        { }


        public NetworkMessageContainer(
            MessagePackSerializableObject data,
            MessageTypes messageType,
            RoutingData routingData = null
        )
        {
            MessageData = data;
            MessageType = messageType;
            RoutingData = routingData;
        }

        public override byte[] Serialize()
        {
            if (!_messageTypeSet)
                throw new RequiredParameterNotInitialized("MessageType", this);


            return base.Serialize();
        }

        public override void WriteToLidgrenMessage_ToClient(MessageTypes messageType, NetOutgoingMessage msg)
        {
            byte[] rawBytes = Serialize();
            msg.Write(Crc32.Compute(rawBytes));
            msg.Write(rawBytes.Length);
            msg.Write(rawBytes);
        }

    }
}
