namespace Freecon.Server.Core.Subscriptions
{
    public class ClientStatusChanged
    {
        public IClientNode Client { get; private set; }

        public ClientStatusChangeType Type { get; private set; }

        public ClientStatusChanged(IClientNode client, ClientStatusChangeType type)
        {
            Client = client;
            Type = type;
        }
    }

    public enum ClientStatusChangeType
    {
        Connected,
        Disconnected
    }
}
