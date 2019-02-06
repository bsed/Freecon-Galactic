using MongoDB.Driver;
using Server.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freecon.Core.Interfaces;
using RedisWrapper;
using Server.Models;
using Server.Interfaces;
using Server.Models.Interfaces;

namespace Server.Managers
{
    public class DBStateConsistencyManager : MongoDatabaseManager
    {
        protected RedisServer _redisServer;

        public DBStateConsistencyManager(RedisServer redisServer)
        {
            _redisServer = redisServer;
        }

        public override async Task<ReplaceOneResult> SaveAsync(ISerializable obj)
        {
            Player pl = obj as Player;
            if (pl != null)
            {                
                await _redisServer.SetHashValue(RedisDBKeyTypes.PlayerIDToCurrentAreaID, obj.Id, pl.CurrentAreaID);
            }

            return await base.SaveAsync(obj);
        }
           
        /// <summary>
        /// This will fail if objects in objList have different model types!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList"></param>
        /// <returns></returns>
        public override async Task<BulkWriteResult> SaveAsyncBulk<T>(IEnumerable<T> objList)
        {
            foreach(var o in objList)
            {
                var pl = o as Player;
                if (pl != null)
                {
                    await _redisServer.SetHashValue(RedisDBKeyTypes.PlayerIDToCurrentAreaID, o.Id, pl.CurrentAreaID);
                }
            }
            return await base.SaveAsyncBulk<T>(objList);
        }

    }
}
