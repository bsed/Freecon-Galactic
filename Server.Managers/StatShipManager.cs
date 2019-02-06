using System.Collections.Generic;
using System.Linq;
using Freecon.Models.TypeEnums;
using Core.Models;

namespace Server.Managers
{

    public class ShipStatManager
    {
        public static List<ShipStats> StatShipList;

        /// <summary>
        /// Used for finding client-synced ships
        /// </summary>
        public static Dictionary<ShipTypes, ShipStats> TypeToStats;

        public static void ReadShipsFromDBSList(IEnumerable<ShipStats> databaseOfShips)
        {
            StatShipList = new List<ShipStats>(databaseOfShips.Count());
            TypeToStats = new Dictionary<ShipTypes, ShipStats>();

            foreach(ShipStats s in databaseOfShips)
                AddShipToList(s);
            
        }

        public static void AddShipToList(ShipStats shs)
        {
            StatShipList.Add(shs);

            // If we're a synced ship, add us to the list of referencable ships.
            if (shs.ShipType != ShipTypes.CustomShip)
                if (!TypeToStats.ContainsKey(shs.ShipType)) // Validate we don't already exist
                {
                    TypeToStats.Add(shs.ShipType, shs);
                }
        }
             


        ///// <summary>
        ///// This may crash the game when you remove a ship. Adding ships should be fine though.
        ///// </summary>
        ///// <param name="databaseOfShips">Ship Database</param>
        //public static void UpdateShipsFromDBSList(List<ShipStats> databaseOfShips)
        //{
        //    // We find ships that are edited or deleted.
        //    // Then we keep track of the ships that still exist.
        //    // All ships that aren't found are then dumped into the ShipList
        //    var ShipsFound = new List<ShipStats>();
        //    for (int i = 0; i < ShipList.Count; i++)
        //    {
        //        for (int db = 0; db < databaseOfShips.Count; db++)
        //        {
        //            if (!databaseOfShips[db].PlayerShip) // Ignore things like motherships
        //                continue;
        //            if (ShipList[i].IsSameID(databaseOfShips[db]))
        //                // Check if ships match up, and if they don't update existing.
        //                // Players pull directly from here, so any updates are made immediate on next warp.
        //                if (!ShipList[i].CheckStatsAgainstDB(databaseOfShips[db]))
        //                {
        //                    ShipList[i] = ConvertFromDatabaseShip(databaseOfShips[db]);
        //                    ShipsFound.Add(ShipList[i]);
        //                    break;
        //                }
        //                else
        //                    ShipsFound.Add(ConvertFromDatabaseShip(databaseOfShips[db]));
        //            // IShip was deleted most likely
        //            if (db == databaseOfShips.Count)
        //            {
        //                ShipList.Remove(ShipList[i]);
        //                if (TypeToStats.ContainsKey(ShipList[i].ShipType))
        //                    TypeToStats.Remove(ShipList[i].ShipType);
        //                i--;
        //            }
        //        }
        //    }

        //    // Checks to find which ships haven't been added.
        //    for (int db = 0; db < databaseOfShips.Count; db++)
        //    {
        //        if (!databaseOfShips[db].PlayerShip) // Ignore things like motherships
        //            continue;
        //        for (int i = 0; i < ShipsFound.Count; i++)
        //        {
        //            // If the IShip is in the list, remove it from the list of found ships
        //            if (ShipsFound[i].IsSameID(databaseOfShips[db]))
        //            {
        //                ShipsFound.RemoveAt(i);
        //                break;
        //            }
        //            // If it's a new ship, lets add it.
        //            if (i == ShipsFound.Count)
        //            {
        //                ShipList.Add(ConvertFromDatabaseShip(databaseOfShips[db]));

        //                // Add to lookup list
        //                if (!TypeToStats.ContainsKey(ShipList[ShipList.Count - 1].ShipType))
        //                    TypeToStats.Add(ShipList[ShipList.Count - 1].ShipType, ShipList[ShipList.Count - 1]);
        //            }
        //        }
        //    }
        //}

    }
}