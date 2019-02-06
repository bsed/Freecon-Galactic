using System.Threading.Tasks;
using Freecon.Core.Utils;
using Freecon.Server.Core.Interfaces;
using RedisWrapper;

namespace Freecon.Server.Core.Services
{
    public class WebServerConfigService : IServerService, IServerConfigService
    {
        public int CurrentServiceId { get; protected set; }

        private readonly RedisServer _redis;

        public FreeconServiceType ServiceType { get { return FreeconServiceType.Slave; } }

        public WebServerConfigService(RedisServer redis, int currentServiceId)
        {
            CurrentServiceId = currentServiceId;
            _redis = redis;
        }
        
    }
}
