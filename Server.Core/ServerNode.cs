using Freecon.Core.Networking;
using Freecon.Core.Networking.Models;
using System;
using Freecon.Core.Utils;

namespace Freecon.Server.Core
{
    public class ServerNode : IServerNode
    {
        protected IServerTransportBroker _broker;
        protected IMessageSerializer _messageSerializer;
        protected ILoggerUtil _logger;

        public IObservable<RawClientRequest> MessageStream { get; private set; }

        public ServerNode(
            IServerTransportBroker broker,
            IMessageSerializer messageSerializer,
            ILoggerUtil logger)
        {
            _broker = broker; 
            _logger = logger;
            _messageSerializer = messageSerializer;
        }

        public IObservable<RawClientRequest> Start()
        {
            var transportNode = _broker.Start(null);

            MessageStream = transportNode.ReceivedMessage;

            return MessageStream;
        }

        public void Stop()
        {
            _broker.Stop();
        }

        public void SendMessage(ICommMessage message)
        {

        }

        public void SendMessage(ClientResponse response)
        {

        }
    }

    public interface IServerNode
    {
        IObservable<RawClientRequest> MessageStream { get; }

        IObservable<RawClientRequest> Start();

        void Stop();

        void SendMessage(ICommMessage message);

        void SendMessage(ClientResponse response);
    }
}
