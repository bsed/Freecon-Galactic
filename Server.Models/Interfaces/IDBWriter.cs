using System.Collections.Generic;
using System.Threading.Tasks;
using Freecon.Core.Interfaces;
using MongoDB.Driver;

namespace Server.Models.Interfaces
{
    public interface IDBWriter
    {
        /// <summary>
        /// Inserts the object into the database if it doesn't already exist, updates the object otherwise.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveChanges">Force db write. Set to false when sequentially inserting multiple objects. Manual context.SaveChanges required after.</param>
        /// <returns></returns>
        Task<ReplaceOneResult> SaveAsync(ISerializable obj);

        /// <summary>
        /// This will fail if objects in objList have different model types!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objList"></param>
        /// <returns></returns>
        Task<BulkWriteResult> SaveAsyncBulk<T>(IEnumerable<T> objList)
            where T : ISerializable;

        Task<DeleteResult> DeleteTeam(int id);

        Task<ReplaceOneResult> HandoffSaveAsync(ISerializable obj);

        Task<DeleteResult> HandoffDeleteAsync(int id, ModelTypes modelType);

        Task<DeleteResult> DeleteAsync(int id, ModelTypes modelType);

        Task<DeleteResult> DeleteAsync(string id, ModelTypes modelType);
    }
}
