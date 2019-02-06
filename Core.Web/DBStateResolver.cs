using RedisWrapper;
using Server.Managers;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Web
{
    public class DBStateResolver:DBStateConsistencyManager
    {
        public DBStateResolver(RedisServer redisServer):base(redisServer)
        {

        }

        public override async Task<PlayerModel> GetPlayerAsync(int ID)
        {
            var dbp = await base.GetPlayerAsync(ID);

            await SetCurrentAreaIfExists(dbp);

            return dbp;
        }

        public override async Task<PlayerModel> GetPlayerAsync(string username)
        {
            var dbp = await base.GetPlayerAsync(username);
            if (dbp != null)
            {
                await SetCurrentAreaIfExists(dbp);

                return dbp;
            }
            else
            {
                return null;
            }
        }

        public override async Task<IEnumerable<PlayerModel>> GetPlayersAsync(IEnumerable<int> ids)
        {
            var res = await base.GetPlayersAsync(ids);
            foreach (var pl in res)
            {
                await SetCurrentAreaIfExists(pl);
            }

            return res;
        }


        async Task SetCurrentAreaIfExists(PlayerModel pl)
        {
            var currentAreaID = await _redisServer.GetHashValue(RedisDBKeyTypes.PlayerIDToCurrentAreaID, pl.Id);
            if (!currentAreaID.IsNullOrEmpty)
            {
                pl.CurrentAreaID = (int?)currentAreaID;
            }
        }

    }
}
