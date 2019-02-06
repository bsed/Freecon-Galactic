using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Interfaces
{
    public interface IDatabaseManager
    {

        Ship GetShip(int id);

        Area GetArea(int id);

        Player GetPlayer(int id);

        Account GetAccount(int id);

        IEnumerable<ShipStats> GetStatsFromDB();

        int GlobalQuery(string q);

        /// <summary>
        /// Adds the object to the database and the db context to automatically track changes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveChanges">Force db write. Set to false when sequentially inserting multiple objects. Manual context.SaveChanges required after.</param>
        /// <returns></returns>
        T Add<T>(T obj);

        IEnumerable<T> Add<T>(IEnumerable<T> objects);

        /// <summary>
        /// Deletes an object from the db
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveChanges">Force db write. Set to false when sequentially inserting multiple objects. Manual context.SaveChanges required after.</param>
        /// <returns></returns>
        T Delete<T>(T obj);
        
        IEnumerable<T> Delete<T>(IEnumerable<T> objects);

        IEnumerable<PlanetLayout> ReadLayouts();

#if DEBUG
        /// <summary>
        /// Probably shouldn't use this after release.
        /// </summary>
        void ClearDatabase(string args);
#endif
    }
}
