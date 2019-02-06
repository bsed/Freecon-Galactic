using AutoMapper;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Proto;
using ProtoBuf;
using System;
using System.IO;

namespace Freecon.Core.Networking.Proto
{
    public class ProtobufMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// Deserializes a Protobuf message.
        /// </summary>
        /// <typeparam name="TProto">Protobuf member implementing IExtensible.</typeparam>
        /// <param name="raw">Bytes to deserialize instance from.</param>
        /// <returns><typeparamref name="TProto"/> instance</returns>
        public virtual TProto DeserializeProtobuf<TProto>(byte[] raw) where TProto : IExtensible
        {
            return Serializer.Deserialize<TProto>(new MemoryStream(raw));
        }

        public virtual TOutput DeserializeProtobufAndMap<TProto, TOutput>(byte[] raw)
            where TProto : IExtensible
            where TOutput : ICommMessage
        {
            return MapProto<TProto, TOutput>(DeserializeProtobuf<TProto>(raw));
        }

        /// <summary>
        /// Returns a RawMessageContainer containing the deserialized payload.
        /// </summary>
        /// <param name="raw">Byte array to deserialize from.</param>
        /// <returns>RawMessageContainer instance.</returns>
        public virtual RawMessageContainer DeserializeMessageContainer(byte[] raw)
        {
            return DeserializeProtobufAndMap<MessageContainerProto, RawMessageContainer>(raw);
        }

        /// <summary>
        /// Deserializes and maps Protos to Payloads.
        /// Note: You have to register new message types with the Server router in ServerBootstrapper.cs
        /// </summary>
        /// <param name="container">RawMessageContainer to read from.</param>
        /// <returns>Deserialized ICommMessage object.</returns>
        public virtual ICommMessage DeserializePayload(RawMessageContainer container)
        {
            // We need to keep this switch here because we can't deserialize the Protobuf without a type.
            // Well, actually... We can. Protobuf.Serializer.NonGeneric provides a set of methods that read
            // The attribute tags from a Protobuf object and deserializes it. But it's slow.
            // And this is networking. So optimizing for speed is the best idea for us. Reflection ain't good, yo.
            // The primary reason this switch is here is so that ServerNode doesn't have to keep a reference to
            // The proto objects. Since they require a reference to Protobuf.Net
            // and we don't want to force ourselves to keep reference that in every project.
            switch (container.PayloadType)
            {
                case TodoMessageTypes.PositionUpdateData:
                    return DeserializeProtobufAndMap<PositionUpdateProto, PositionUpdate>(container.Data);
            }

            // Todo: Throw custom exceptions.
            throw new Exception("Deserialization exception");
        }

        public byte[] GetBytesFromProto<T>(T proto) where T : IExtensible
        {
            var s = new MemoryStream();

            Serializer.Serialize<T>(s, proto);

            return s.ToArray();
        }

        public byte[] MapAndGetBytesFromProto<TSource, TDestination>(ICommMessage message)
            where TSource : ICommMessage
            where TDestination : IExtensible
        {
            return GetBytesFromProto<TDestination>(Mapper.Map<TSource, TDestination>((TSource)message));
        }

        public virtual TOutput MapProto<TProto, TOutput>(TProto payload)
            where TProto : IExtensible
            where TOutput : ICommMessage
        {
            return Mapper.Map<TProto, TOutput>(payload);
        }

        /// <summary>
        /// Serializes an ICommMessage to a serialize Protobuf byte array, by mapping,
        /// Serializing, and creating a RawMessageContainer for transport.
        /// </summary>
        /// <param name="message">ICommMessage instance to </param>
        /// <returns>Serialized bytes.</returns>
        public byte[] SerializeMessageContainer(ICommMessage message)
        {
            return SerializeMessageContainer(message.PayloadType, SerializePayload(message));
        }

        /// <summary>
        /// Creates a RawMessageContainer from the parameters.
        /// </summary>
        /// <param name="type">Type of message to pass to container.</param>
        /// <param name="payload">Raw, serialized payload in bytes.</param>
        /// <returns>Serialized bytes.</returns>
        public byte[] SerializeMessageContainer(TodoMessageTypes type, byte[] payload)
        {
            var messageContainer = new RawMessageContainer(type, payload);

            var rawProtoContainer = Mapper.Map<RawMessageContainer, MessageContainerProto>(messageContainer);

            return GetBytesFromProto<MessageContainerProto>(rawProtoContainer);
        }

        /// <summary>
        /// Serializes an ICommMessage by mapping to Protobuf, then serializing.
        /// </summary>
        /// <param name="message">ICommMessage to map and serialize.</param>
        /// <returns>Raw byte array of serialized Protobuf message.</returns>
        public byte[] SerializePayload(ICommMessage message)
        {
            switch (message.PayloadType)
            {
                case TodoMessageTypes.PositionUpdateData:
                    return MapAndGetBytesFromProto<PositionUpdate, PositionUpdateProto>(message);
            }

            // Todo: Throw custom exceptions.
            throw new Exception("Serialization exception");
        }
    }
}
