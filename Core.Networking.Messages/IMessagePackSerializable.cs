//using Lidgren.Network;
//using MsgPack.Serialization;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Freecon.Core.Utils;
//using Newtonsoft.Json;

//namespace Freecon.Core.Networking.Models
//{
//    /// <summary>
//    /// Represents classes which may be directly packed/unpacked with MessagePack.
//    /// </summary>
//    public abstract class MessagePackSerializableObject
//    {
//        /// <summary>
//        /// Writes the message type header to the message along with the data.
//        /// </summary>
//        /// <param name="messageType"></param>
//        /// <param name="msg"></param>
//        public virtual void WriteToLidgrenMessage_ToServer(MessageTypes messageType, NetOutgoingMessage msg)
//        {
//            byte[] rawBytes = Serialize();
//            msg.Write(Crc32.Compute(rawBytes));
//            msg.Write(rawBytes.Length);
//            msg.Write(rawBytes);
//        }
        
//        /// <summary>
//        /// Possibly temporary, but I don't like the idea of having to cast every message when processing messages on the server
//        /// </summary>
//        /// <param name="messageType"></param>
//        /// <param name="msg"></param>
//        public virtual void WriteToLidgrenMessage_ToClient(MessageTypes messageType, NetOutgoingMessage msg)
//        {
//            var container = new NetworkMessageContainer();
//            container.MessageType = messageType;
//            container.MessageData = this;
//            byte[] rawBytes = container.Serialize();
//            msg.Write(Crc32.Compute(rawBytes));
//            msg.Write(rawBytes.Length);
//            msg.Write(rawBytes);

//        }
         

//#if DEBUG
//        public virtual byte[] Serialize()
//        {
//            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
//            var json = JsonConvert.SerializeObject(this, settings);
//            return Encoding.ASCII.GetBytes(json);
//        }

//#else
//        public virtual byte[] Serialize()
//        {
//            var serializer = MessagePackSerializer.Get(this.GetType());
            
//            var stream = new MemoryStream();
//                serializer.Pack(stream, this);
//                 return stream.GetBuffer();
//        }
//#endif   

//    }

//    public class NetworkMessageContainer:MessagePackSerializableObject
//    {
//        public MessageTypes MessageType { get { return _messageType; } set { _messageType = value; _messageTypeSet = true; } }
//        MessageTypes _messageType;
//        bool _messageTypeSet;
             
//        /// <summary>
//        /// Currently only used for routing redis based simulator bound messages
//        /// </summary>
//        public int? RoutingTargetId { get; set; }

//        [MessagePackRuntimeType]
//        [JsonProperty]
//        public MessagePackSerializableObject MessageData { get; set; }

//        public NetworkMessageContainer()
//        { }

     
//        public NetworkMessageContainer(
//            MessagePackSerializableObject data,
//            MessageTypes messageType,
//            int? targetAreaID = null
//        )
//        {
//            MessageData = data;
//            MessageType = messageType;
//            RoutingTargetId = targetAreaID;
//        }

//        public override byte[] Serialize()
//        {
//            if (!_messageTypeSet)
//                throw new RequiredParameterNotInitialized("MessageType", this);

            
//            return base.Serialize();
//        }

//        public override void WriteToLidgrenMessage_ToClient(MessageTypes messageType, NetOutgoingMessage msg)
//        {
//            byte[] rawBytes = Serialize();
//            msg.Write(Crc32.Compute(rawBytes));
//            msg.Write(rawBytes.Length);
//            msg.Write(rawBytes);
//        }

//    }
//}
