using Server.Models;
using Server.Models.Interfaces;
using SRServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Server.Interfaces
{
    public interface IDatabaseManager
    {    
        
        /// <summary>
        /// Returns null if ship is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Ship GetShip(int id, IPlayerLocator pl, IAreaLocator al, ITeamLocator tl);

        public IEnumerable<Ship> GetAllShips(IPlayerLocator pl, IAreaLocator al, ITeamLocator tl);

        /// <summary>
        /// Returns null if area is not found. Will probably be depreciated soon, because of excessive querying.
        /// Note that this method does not load nested references if they are stored as Ids (e.g. planets in systems, which are stored only by IDs: PSystem.PlanetIDs) 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Area GetArea(int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm, IGalaxyRegistrationManager rm, ITeamLocator tl);

        public Planet GetPlanet(int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm, IGalaxyRegistrationManager rm, ITeamLocator tl);
        
        public Moon GetMoon(int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm);
        
        public PSystem GetSystem(int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm);
        
        public Port GetPort(int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm);
        
        public Colony GetColony(int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm);

        /// <summary>
        /// Does not load or register nested objects within systems (e.g. planets)
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="al"></param>
        /// <param name="sl"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        public IEnumerable<PSystem> GetAllSystems(IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm);

        /// <summary>
        /// Returns null if player not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Player GetPlayer(int id, IAreaLocator al, IShipLocator sl, IAccountLocator acl, ITeamLocator tl, ITeamManager tm);

        public IEnumerable<Player> GetAllPlayers(IAreaLocator al, IShipLocator sl, IAccountLocator acl, ITeamLocator tl, ITeamManager tm);


        /// <summary>
        /// Returns null if account not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Account GetAccount(int id);

        public IEnumerable<Account> GetAllAccounts();

        public Team GetTeam(int id);

        public void DeleteTeam(int id);

        public IEnumerable<ShipStats> GetStatsFromDB();

        /// <summary>
        /// Adds the object to the database and the db context to automatically track changes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveChanges">Force db write. Set to false when sequentially inserting multiple objects. Manual context.SaveChanges required after.</param>
        /// <returns></returns>
        public IEFSerializable Add(IEFSerializable obj, bool saveChanges = true);
        
        public IEnumerable<IEFSerializable> Add(IEnumerable<IEFSerializable> objects);

        /// <summary>
        /// Deletes an object from the db
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveChanges">Force db write. Set to false when sequentially inserting multiple objects. Manual context.SaveChanges required after.</param>
        /// <returns></returns>
        public IEFSerializable Delete(IEFSerializable obj, bool saveChanges = true);

        public IEnumerable<IEFSerializable> Delete(IEnumerable<IEFSerializable> objects);
     
        public IEnumerable<PlanetLayout> ReadLayouts();
 
      




    }
}
