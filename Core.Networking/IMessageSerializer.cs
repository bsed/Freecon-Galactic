using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Networking
{
    public interface IMessageSerializer
    {
        RawMessageContainer DeserializeMessageContainer(byte[] raw);

        ICommMessage DeserializePayload(RawMessageContainer container);

        byte[] SerializePayload(ICommMessage message);

        byte[] SerializeMessageContainer(ICommMessage message);

        byte[] SerializeMessageContainer(TodoMessageTypes type, byte[] payload);
    }

    /// <summary>
    /// A container that holds a deserialized payload and it's type.
    /// </summary>
    public class MessageContainer
    {
        public MessageTypes Type { get; private set; }

        /// <summary>
        /// Deserialized payload that's boxed.
        /// </summary>
        public ICommMessage Message { get; private set; }

        public MessageContainer(MessageTypes type, ICommMessage message)
        {
            Type = type;
            Message = message;
        }
    }
}
