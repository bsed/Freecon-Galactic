using Core.Models.Enums;
using Freecon.Server.Configs;
using MasterServer;
using RedisWrapper;
using Server.Database;
using Server.Factories;
using Server.GlobalIDManagers;
using Server.Managers;
using SRServer.Debug;
using SRServer.Services;
using System.Collections.Generic;
using System.Linq;
using Core.Logging;
using Freecon.Server.Core.Interfaces;
using Server.Models;
using Server.MongoDB;

namespace DBFiller
{
    /// <summary>
    /// Minimal collection of managers and initialization code required for filling the DB without corrupting state.
    /// </summary>
    public class MinimalMockServer
    {
        public LocalIDManager GalaxyIDManager;
        public LocalIDManager TeamIDManager;
        public LocalIDManager accountIDManager;
        public GlobalGalaxyIDManager globalGalaxyIDManager;
        public GlobalTeamIDManager globalTeamIDManager;
        public GlobalAccountIDManager globalAccountIDManager;

        public RedisServer _redisServer;
        public GlobalTeamManager TeamManager;
        public PlayerManager PlayerManager;
        public GalaxyManager GalaxyManager;
        public ShipManager shipManager;
        public CollisionManager CollisionManager;
        public IDatabaseManager DatabaseManager;
        public IDbIdIoService DbIdIoService;
        public DebugGalaxyGenerator GalaxyGenerator;
        public AccountManager_MasterServer AccountManager;
        public GalaxyRegistrationManager RegistrationManger;
        public CargoSynchronizer CargoSynchronizer;
        public StructureManager StructureManager;
        public WarpManager WarpManager;

        public LocatorService LocatorService;

        //For convenience.
        public IEnumerable<Player> HumanPlayers;

        public MinimalMockServer(GalacticProperties gp, IDatabaseManager databaseManager, IDbIdIoService dbIdIoService)
        {
            Logger.Initialize();

            DatabaseManager = databaseManager;
            DbIdIoService = dbIdIoService;

            //Minimal initializations
            GalaxyIDManager = new LocalIDManager(null, IDTypes.GalaxyID);
            TeamIDManager = new LocalIDManager(null, IDTypes.TeamID);
            accountIDManager = new LocalIDManager(null, IDTypes.AccountID);

            InitializeIdData(gp);

            globalGalaxyIDManager = new GlobalGalaxyIDManager(dbIdIoService, gp);
            GenerateIDsForLocalIDManager(globalGalaxyIDManager, GalaxyIDManager, gp.IdProperties[IDTypes.GalaxyID].LastIDAdded);

            globalTeamIDManager = new GlobalTeamIDManager(dbIdIoService, gp);
            GenerateIDsForLocalIDManager(globalTeamIDManager, TeamIDManager, gp.IdProperties[IDTypes.TeamID].LastIDAdded);

            globalAccountIDManager = new GlobalAccountIDManager(dbIdIoService, gp);
            GenerateIDsForLocalIDManager(globalAccountIDManager, accountIDManager, gp.IdProperties[IDTypes.AccountID].LastIDAdded);

            _redisServer = new RedisServer(Logger.LogRedisError, Logger.LogRedisInfo, new RedisConfig().Address);


            TeamManager = new GlobalTeamManager(TeamIDManager, null, null, null, DatabaseManager);
            PlayerManager = new PlayerManager(DatabaseManager, null, _redisServer, GalaxyIDManager, null);
            GalaxyManager = new GalaxyManager(gp.SolID, TeamManager);
            shipManager = new ShipManager(null, GalaxyManager, null, null, DatabaseManager);
            CollisionManager = new CollisionManager(GalaxyManager, null, null, null);
            GalaxyGenerator = new DebugGalaxyGenerator(PlayerManager, GalaxyManager, shipManager);
            AccountManager = new AccountManager_MasterServer(accountIDManager, DatabaseManager, false);
            CargoSynchronizer = new Server.Managers.CargoSynchronizer();
            StructureManager = new StructureManager(DatabaseManager, GalaxyManager, GalaxyIDManager, CargoSynchronizer);
            RegistrationManger = new GalaxyRegistrationManager(GalaxyManager, shipManager, CollisionManager, GalaxyIDManager, PlayerManager, null, CargoSynchronizer, StructureManager);
            ColonyFactory.Initialize(GalaxyIDManager, RegistrationManger);
            StructureFactory.Initialize(GalaxyIDManager, RegistrationManger);
            LocatorService = new LocatorService(RegistrationManger, PlayerManager, GalaxyManager, shipManager, AccountManager, TeamManager, TeamManager, null, StructureManager, null);

            WarpManager = new WarpManager(GalaxyManager, null, null, _redisServer, AccountManager, null);

            CargoSynchronizer.Start(10, 4);
        }
        
        public void KillThreads()
        {
            CargoSynchronizer.Stop();
        }


        void InitializeIdData(GalacticProperties gp)
        {
            foreach (var idd in gp.IdProperties.Values)
            {
                IdData idData = new IdData(idd.IDType,
                idd.LastIDAdded,
                new HashSet<int>(Enumerable.Range(0, idd.LastIDAdded + 1).Where(i => !idd.ReservedIDs.Contains(i))),
                new HashSet<int>(),
                idd.ReservedIDs);

                DbIdIoService.SaveIdDataAsync(idData).Wait();
            }
        }
        
        /// <summary>
        /// Simulates IDs which would be generated by the master server, to allow for ship creation
        /// </summary>
        /// <param name="numIDs"></param>
        void GenerateIDsForLocalIDManager(GlobalIDManager globalIDManager, LocalIDManager galaxyIDManager, int numIDs)
        {
            var IDs = globalIDManager.GetFreeIDs(numIDs);
            galaxyIDManager.ReceiveServerIDs(IDs);
        }

        

    }
}
