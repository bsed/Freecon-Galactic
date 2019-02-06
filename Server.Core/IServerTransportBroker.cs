using Freecon.Server.Core.Subscriptions;
using System;

namespace Freecon.Server.Core
{
    public interface IServerTransportBroker
    {
        IServerTransportNode Start(Func<IClientNode, bool> validateClientCallback);

        void Stop();
    }

    public class ServerTransportNode : IServerTransportNode
    {
        public IObservable<RawClientRequest> ReceivedMessage { get; private set; }

        public IObservable<ClientStatusChanged> ClientStatusChanged { get; private set; }

        public IObservable<ServerStatusChanged> ServerStatusChanged { get; private set; }

        public ServerTransportNode(IObservable<RawClientRequest> receivedMessage,
                                   IObservable<ClientStatusChanged> clientStatusChanged,
                                   IObservable<ServerStatusChanged> serverStatusChanged)
        {
            ReceivedMessage = receivedMessage;
            ClientStatusChanged = clientStatusChanged;
            ServerStatusChanged = serverStatusChanged;
        }
    }

    public interface IServerTransportNode
    {
        IObservable<RawClientRequest> ReceivedMessage { get; }

        IObservable<ClientStatusChanged> ClientStatusChanged { get; }

        IObservable<ServerStatusChanged> ServerStatusChanged { get; }
    }
}
