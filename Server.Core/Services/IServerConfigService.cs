namespace Freecon.Server.Core.Services
{
    public interface IServerConfigService
    {
        int CurrentServiceId { get; }

        FreeconServiceType ServiceType { get; }
    }

    public enum FreeconServiceType
    {
        Slave,
        Web
    }
}