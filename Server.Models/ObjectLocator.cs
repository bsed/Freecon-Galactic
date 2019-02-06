using Server.Models;
using Server.Models.Space;
using Server.Models.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Freecon.Core.Interfaces;
using Server.Interfaces;
using Server.Models.Structures;

namespace SRServer.Services
{
    public interface IPlayerLocator
    {
        Task<Player> GetPlayerAsync(int? ID, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true);

        Task<ICollection<Player>> GetPlayersAsync(List<int> IDs, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true);
    }

    public interface IAreaLocator
    {
        int? SolAreaID { get; }

        /// <summary>
        /// NOTE: If ID==null, GetArea must return limbo
        /// </summary>
        /// <param name="areaID"></param>
        /// <returns></returns>
        IArea GetArea(int? areaID);

        /// <summary>
        /// Checks if the area is being handled by this server
        /// returns true if systemID is null (null areaID points to a Limbo instance)
        /// </summary>
        /// <param name="systemID"></param>
        /// <returns></returns>
        bool IsLocalSystem(int? systemID);

        /// <summary>
        /// Checks if an areaId references a locally handled area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        bool IsLocalArea(int? areaId);

    }

    public interface IObjectLocator<ObjectType>
    {
        Task<ObjectType> GetObjectAsync(int? ID, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true);

        Task<ICollection<ObjectType>> GetObjectsAsync(List<int> IDs, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true);
    }

    public interface IShipLocator
    {
        Task<IShip> GetShipAsync(int? ID, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true);

        Task<ICollection<IShip>> GetShipsAsync(IEnumerable<int> IDs, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true);
    }
       

    public interface IAccountLocator
    {
        Task<Account> GetAccountAsync(int? accountID, bool fetchFromDB = true, bool persistFetch = true);

        Task<ICollection<Account>> GetAccountsAsync(IEnumerable<int> ids, bool fetchFromDB = true, bool persistFetch = true);
    }

    public interface ITeamLocator
    {
        bool AreAllied(ITeamable T1, ITeamable T2);
    }

    public interface IWarpManager
    {
        Task ChangeArea(int newAreaID, IShip ship, bool isWarping, bool writeToDb);
    }

    public class LocatorService
    {        
        public IPlayerLocator PlayerLocator;
        public IAreaLocator AreaLocator;
        public IShipLocator ShipLocator;
        public IAccountLocator AccountLocator;
        public IObjectLocator<IStructure> StructureManager;
        public ITeamLocator TeamLocator;
        public ITeamManager TeamManager;//Not sure if this belongs here...
        public IMessageManager MessageManager;//Not sure if this belongs here...
        public IGalaxyRegistrationManager RegistrationManager;
        public ISlaveIDProvider SlaveIDProvider;

        public LocatorService(IGalaxyRegistrationManager grm, 
            IPlayerLocator pl, 
            IAreaLocator al, 
            IShipLocator sl, 
            IAccountLocator acl, 
            ITeamLocator tl, 
            ITeamManager tm, 
            IMessageManager mm, 
            IObjectLocator<IStructure> structureManager, 
            ISlaveIDProvider slaveIDProvider)
        {
            PlayerLocator = pl;
            ShipLocator = sl;
            AccountLocator = acl;
            TeamLocator = tl;
            TeamManager = tm;
            AreaLocator = al;
            MessageManager = mm;
            RegistrationManager = grm;
            StructureManager = structureManager;
            SlaveIDProvider = slaveIDProvider;
        }


    }
    /// <summary>
    /// This will probably be removed eventually.
    /// </summary>
    public class DummyRegistrationManager : IGalaxyRegistrationManager
    {
        public void RegisterObject(IHasGalaxyID obj)
        { }

        public void RegisterObject(Player p)
        { }

        public void RegisterObject(Account a)
        { }

        public void DeRegisterObject(IHasGalaxyID obj)
        { }

        public void DeRegisterObject(Player p)
        { }

    }
}
