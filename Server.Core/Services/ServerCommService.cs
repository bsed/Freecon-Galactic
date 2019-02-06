using Freecon.Core.Configs;
using Freecon.Core.Networking.Models;
using System;
using Freecon.Core.Networking.Proto;
using Freecon.Server.Core.Subscriptions;
using Freecon.Core.Networking;
using Freecon.Core.Utils;

namespace Freecon.Server.Core.Services
{
    public class ServerCommService : IServerTransportService, IMessageSendingService, IMessageStreamService
    {
        private IMessageSerializer _messageSerializer;
        private ILoggerUtil _logger;
        private MapperConfig _mapperConfig;

        public ServerNode ClientServerInstance { get; private set; }

        public ServerCommService(
            MapperConfig mapperConfig, 
            IMessageSerializer messageSerializer,
            ILoggerUtil logger
            )
        {
            _messageSerializer = messageSerializer;
            _mapperConfig = mapperConfig;
            _logger = logger;
        }

        public ServerNode StartServer(INetworkConfig netConfig)
        {
            //var transportBroker = new ServerTransportBroker(netConfig, _logger);

            //var serverNode = new ServerNode(transportBroker, _messageDeserializer, _logger);

            //serverNode.Start();

            //ClientServerInstance = serverNode; // Figure this out :(

            //return serverNode;
            return null;
        }

        public void SendMessage<TSend>(IClient client, TSend message) where TSend : ICommMessage
        {
            // Setup priority mapping for every send.
            // For example, if we want to send 'ReliableSequenced', our code shouldn't know.
        }

        public IObservable<RawClientRequest> RawMessageStream
        {
            get { return ClientServerInstance.MessageStream; }
        }

        public IObservable<ClientStatusChanged> ClientStatus
        {
            get { throw new NotImplementedException(); }
        }

        public IObservable<TSubscribe> Subscribe<TSubscribe>() where TSubscribe : class, ICommMessage
        {
            // Todo: make this safe

            var type = _mapperConfig.TypeToMessageEnum[typeof(TSubscribe)];

            return null;
            //return from n in ClientServerInstance.MessageStream
            //       where n.Message.PayloadType == type
            //       select n.Message as TSubscribe;

            // Todo: Create a stream from this.
            //return ClientServerInstance.MessageStream
                //.Where((clientRequest, val) => clientRequest.Message.MessageType == type)
                //.Select(p = >p);
        }
    }

    public interface IMessageStreamService
    {
        /// <summary>
        /// Ideally, you shouldn't use this and should instead subscribe via the Generic methods.
        /// </summary>
        IObservable<RawClientRequest> RawMessageStream { get; }

        IObservable<ClientStatusChanged> ClientStatus { get; }

        IObservable<TSubscribe> Subscribe<TSubscribe>() where TSubscribe : class, ICommMessage;
    }

    public interface IMessageSendingService
    {
        void SendMessage<TSend>(IClient client, TSend message) where TSend : ICommMessage;
    }

    public interface IClient
    {

    }
}
