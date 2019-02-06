using System.Threading.Tasks;
using Freecon.Core.Utils;
using Freecon.Server.Core.Interfaces;
using RedisWrapper;

namespace Freecon.Server.Core.Services
{
    public class SlaveServerConfigService : IServerService, IServerConfigService
    {
        public int CurrentServiceId { get; protected set; }

        private readonly RedisServer _redis;

        public FreeconServiceType ServiceType { get { return FreeconServiceType.Slave; } }

        public SlaveServerConfigService(RedisServer redis, int currentServiceId)
        {
            CurrentServiceId = currentServiceId;
            _redis = redis;
        }

        public static async Task<int> GetFreeSlaveID(RedisServer redis)
        {
            // Overkill, ensures unique slaveID
            int slaveID = Rand.Random.Next(-int.MaxValue, int.MaxValue);

            while (!(await redis.SetHashValue(RedisDBKeyTypes.SlaveIDHashSet, slaveID, slaveID)))
            {
                slaveID = Rand.Random.Next(-int.MaxValue, int.MaxValue);
            }

            return slaveID;
        }
    }
}
