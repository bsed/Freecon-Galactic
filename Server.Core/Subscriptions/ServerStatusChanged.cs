namespace Freecon.Server.Core.Subscriptions
{
    public class ServerStatusChanged
    {
        public ServerStatusChangeType Type { get; private set; }

        public ServerStatusChanged(ServerStatusChangeType type)
        {
            Type = type;
        }
    }

    public enum ServerStatusChangeType
    {
        Starting,
        Started,
        Stopping,
        Stopped
    }
}
