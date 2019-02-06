namespace Freecon.Core.Configs
{
    public interface INetworkConfig
    {
        string Address { get; }

        int Port { get; }

        string ServerName { get; }

        NetworkConfigType Type { get; }
    }
}
