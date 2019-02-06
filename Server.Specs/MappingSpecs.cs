using Autofac;
using AutoMapper;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Proto;
using Freecon.Core.Networking.Proto;
using Freecon.Server.App;
using Freecon.Server.Configs;
using Freecon.Server.Core;
using Machine.Specifications;
using ProtoBuf;
using System;
using System.IO;

namespace Freecon.Server.Specs
{
    public class when_mapping_position_update : base_specs
    { 
        static PositionUpdateProto posUpdateProto;
        static PositionUpdate posUpdate;

        Establish context = () => 
        {
            posUpdateProto = new PositionUpdateProto();
            posUpdateProto.X = 20f;
            posUpdateProto.Y = 121f;
        };

        Because of = () => 
        {
            posUpdate = Mapper.Map<PositionUpdateProto, PositionUpdate>(posUpdateProto);
        };

        It should_have_same_x = () => posUpdate.X.ShouldEqual(posUpdateProto.X);
        It should_have_same_Y = () => posUpdate.Y.ShouldEqual(posUpdateProto.Y);
    }
    public class when_mapping_message_container : base_specs
    {
        static MessageContainerProto mesContainerProto;
        static RawMessageContainer mesContainer;

        Establish context = () =>
        {
            mesContainerProto = new MessageContainerProto();
            mesContainerProto.Data = new byte[2] { 0x42, 0x85 };
            mesContainerProto.PayloadType = 4u;
        };

        Because of = () =>
        {
            mesContainer = Mapper.Map<MessageContainerProto, RawMessageContainer>(mesContainerProto);
        };

        It should_have_same_data = () => mesContainer.Data.ShouldEqual(mesContainerProto.Data);
        It should_have_same_type = () => mesContainer.PayloadType.ShouldEqual((TodoMessageTypes)(mesContainerProto.PayloadType));
    }

    /// <summary>
    /// This holds any setup that you would want to do for every test.
    /// </summary>
    public class base_specs
    {
        protected static ServerBootstrapper Bootstrapper;

        Establish context = () =>
        {
            // Automapper requires an explicit map between types.
            ProtobufMappingSetup.Setup();

            Bootstrapper = new ServerBootstrapper();
        };

        protected static IContainer Setup(Action<ContainerBuilder> setupCallback = null)
        {
            return Bootstrapper.Setup(FreeconServerEnvironment.Test, setupCallback);
        }

        public static RawClientRequest BuildRawRequest<T>(T message, TodoMessageTypes payloadType, IClientNode client)
        {
            var messageStream = new MemoryStream();
            Serializer.Serialize<T>(messageStream, message);

            var container = new MessageContainerProto()
            {
                Data = messageStream.ToArray(),
                PayloadType = (uint)payloadType
            };

            var containerStream = new MemoryStream();
            Serializer.Serialize<MessageContainerProto>(containerStream, container);

            return new RawClientRequest(containerStream.ToArray(), client);
        }
    }
}
