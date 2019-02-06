using System.IO;
using Lidgren.Network;
using MsgPack.Serialization;
using System.Text;
using Core.Models.Enums;
using Freecon.Core.Networking.Objects.CustomSerializers;
using Freecon.Core.Utils;
using Newtonsoft.Json;


namespace Freecon.Core.Networking.Models
{
    /// <summary>
    /// Represents classes which may be directly packed/unpacked with MessagePack.
    /// </summary>
    public abstract class MessagePackSerializableObject
    {
        protected static JsonSerializerSettings _defaultJsonSerializerSettings;

        static MessagePackSerializableObject()
        {
            _defaultJsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            _defaultJsonSerializerSettings.Converters.Add(new IPAddressConverter());
            _defaultJsonSerializerSettings.Converters.Add(new IPEndPointConverter());
            SerializationContext.Default.Serializers.Register(new IPAddressSerializer(SerializationContext.Default));

        }

        /// <summary>
        /// Writes the message type header to the message along with the data.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="msg"></param>
        public virtual void WriteToLidgrenMessage_ToServer(MessageTypes messageType, NetOutgoingMessage msg)
        {
            byte[] rawBytes = Serialize();
            msg.Write(Crc32.Compute(rawBytes));
            msg.Write(rawBytes.Length);
            msg.Write(rawBytes);
        }

        /// <summary>
        /// Possibly temporary, but I don't like the idea of having to cast every message when processing messages on the server
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="msg"></param>
        public virtual void WriteToLidgrenMessage_ToClient(MessageTypes messageType, NetOutgoingMessage msg)
        {
            var container = new NetworkMessageContainer();
            container.MessageType = messageType;
            container.MessageData = this;
            byte[] rawBytes = container.Serialize();
            msg.Write(Crc32.Compute(rawBytes));
            msg.Write(rawBytes.Length);
            msg.Write(rawBytes);

        }


#if DEBUG
        public virtual byte[] Serialize()
        {
            var json = JsonConvert.SerializeObject(this, _defaultJsonSerializerSettings);
            return Encoding.ASCII.GetBytes(json);
        }
#else
        public virtual byte[] Serialize()
        {
            var serializer = MessagePackSerializer.Get(this.GetType());
            var stream = new MemoryStream();
            serializer.Pack(stream, this);
            return stream.GetBuffer();
        }


#endif        

    }

    
}
