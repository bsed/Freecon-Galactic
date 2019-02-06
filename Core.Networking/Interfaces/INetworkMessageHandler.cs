namespace Core.Networking.Interfaces
{
    public interface INetworkMessageHandler
    {
        MessageHandlerID MessageHandlerID { get; }

        bool IsUpdating { get; }

    }
}
