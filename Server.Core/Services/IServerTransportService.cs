using Freecon.Core.Configs;

namespace Freecon.Server.Core.Services
{
    public interface IServerTransportService
    {
        ServerNode StartServer(INetworkConfig netConfig);
    }
}
