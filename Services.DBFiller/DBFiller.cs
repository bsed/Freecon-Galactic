using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SRServer.Services;
using Server.Models;
using Server.Managers;
using Server.Database;
using Freecon.Models.TypeEnums;
using Freecon.Server.Configs;
using Core.Models;
using Server.Managers.Synchronizers.Transactions;
using Server.Factories;
using MasterServer;
using Server.Interfaces;
using Freecon.Models;
using Core.Models.Enums;
using Freecon.Core.Utils;
using Core.Models.CargoHandlers;
using Freecon.Core;
using SRServer.Debug;
using Server.Managers.Factories;

namespace DBFiller
{
    /// <summary>
    /// Bunch of hardcoded data to fill DB with basic, testable data
    /// Pretty hacky
    /// OVERWRITES ENTIRE DB
    /// </summary>
    public class DBFiller
    {
        public List<IShip> ships;
        public List<IArea> Systems;
        IEnumerable<PlanetLayout> layouts;
        public Dictionary<ShipTypes, ShipStats> ShipStats;

        public DBFillerUtils _dbFillerUtils;

        public MinimalMockServer MockServer;

        

        //Configs
        GalacticProperties gp;
        private DBFillerConfig _config;

        IDatabaseManager dbm;

        public DBFiller(DBFillerConfig config, IDatabaseManager databaseManager)
        {
            _config = config;
            gp = new GalacticProperties();
            dbm = databaseManager;
            
            MockServer = new MinimalMockServer(gp, databaseManager, databaseManager);

            _dbFillerUtils = new DBFillerUtils(MockServer.DatabaseManager, MockServer.GalaxyManager, MockServer.LocatorService, MockServer.WarpManager, MockServer.GalaxyIDManager);
        }

       
        public event EventHandler<MinimalMockServer> OnCreationLoadState;
        

        /// <summary>
        /// Creates a working system of Areas, Accounts, Players, and Ships, and fills the DB with them
        /// Careful, FillDB Clears all pre-existing data from DB
        /// Probably best to restart the server and load from DB after calling this method, so method closes the program
        /// </summary>
        /// <param name="dbm"></param>
        public async Task FillDB()
        {     
            await InsertShipStats(dbm);
            var r = await dbm.GetStatsFromDBAsync();
            ShipStatManager.ReadShipsFromDBSList(r);           
           
            layouts = DebugGalaxyGenerator.ReadPlanetLayoutsFromDisk(@"C:\SRDevGit\freecon-galactic");
            await dbm.SaveAsyncBulk(layouts);

            layouts = await dbm.GetAllLayoutsAsync();
            Systems = new List<IArea>();

            List<PSystem> genSys = MockServer.GalaxyGenerator.generateAndFillGalaxy(_config.NumPlanetsPerSystem, gp.SolID, _config.NumSystems, layouts, MockServer.GalaxyManager, MockServer.GalaxyIDManager, MockServer.RegistrationManger, MockServer.LocatorService);

            foreach (PSystem p in genSys)
            {
                MockServer.RegistrationManger.RegisterObject(p);
                Systems.Add(p);
            }

            IEnumerable<Account> accounts = CreateAccounts(MockServer.AccountManager);

            MockServer.HumanPlayers = new List<Player>(CreateHumanPlayers(accounts, MockServer.PlayerManager, MockServer.LocatorService));
            var allPlayers = new List<Player>(MockServer.HumanPlayers);


            await CreateShips(allPlayers, MockServer.GalaxyManager, MockServer.GalaxyIDManager, MockServer.RegistrationManger, MockServer.PlayerManager, MockServer.LocatorService);

            var humanShips = allPlayers.Where(p => p.PlayerType == PlayerTypes.Human).Select(p => p.GetActiveShip());
            await AddCargoToPlayerShips(humanShips, MockServer.GalaxyIDManager, MockServer.RegistrationManger, MockServer.CargoSynchronizer);
            
            _dbFillerUtils.CreateColonies(allPlayers.Where(p=>p.Username == "ALLYOURBASE" || p.Username == "freeqaz"));
            

            _dbFillerUtils.WarpPlayerToOwnedColony(allPlayers.First(p => p.Username == "freeqaz"));
            
            var npcPlayers = await CreateNPCs(MockServer.GalaxyManager, MockServer.GalaxyIDManager, MockServer.RegistrationManger, MockServer.PlayerManager, MockServer.LocatorService, MockServer.CargoSynchronizer, MockServer.TeamManager, MockServer.GalaxyIDManager);
            allPlayers.AddRange(npcPlayers);
            
            foreach (Account a in accounts)
            {
                a.LastSystemID = gp.SolID;
            }

            HashSet<int> IDs = new HashSet<int>();
            foreach (Player p in allPlayers)
            {
                IDs.Add(p.Id);
            }


            AddMines(MockServer.HumanPlayers, new List<PSystem> { (PSystem)MockServer.GalaxyManager.GetArea(MockServer.GalaxyManager.SolAreaID) }, MockServer.HumanPlayers, MockServer.CargoSynchronizer, MockServer.GalaxyManager, MockServer.GalaxyIDManager, MockServer.StructureManager, MockServer.LocatorService);

            await FillPorts(MockServer.GalaxyManager, MockServer.GalaxyIDManager, MockServer.CargoSynchronizer);



            OnCreationLoadState?.Invoke(null, MockServer);

            dbm.SaveAsyncBulk(MockServer.GalaxyManager.GetAllAreas()).Wait();

            dbm.SaveAsyncBulk(accounts).Wait();
            dbm.SaveAsyncBulk(allPlayers).Wait();
            dbm.SaveAsyncBulk(ships).Wait();
            dbm.SaveAsyncBulk(MockServer.StructureManager.GetAllObjects()).Wait();
        }

        async Task FillPorts(GalaxyManager galaxyManager, LocalIDManager galaxyIDManager, CargoSynchronizer cargoSynchronizer)
        {
            var ports = galaxyManager.GetAllAreas().Where(a => a.AreaType == AreaTypes.Port);
            CargoTransaction lastTransaction = null;
            foreach(var p in ports)
            {
                var port = p as Port;
                foreach(var s in _config.PortConfig.StatefulCargoCounts)
                {
                    StatefulCargo sc;
                    for (int i = 0; i < s.Value; i++)//Yes, this loop is lazy, but it's 11:30PM...
                    {
                        //TODO: make a StatefulCargoFactory
                        switch (s.Key)
                        {
                            case StatefulCargoTypes.Barge:
                                {
                                    sc = new CargoShip(galaxyIDManager.PopFreeID(), 666, ShipStats[ShipTypes.Barge]);
                                    break;
                                }
                            case StatefulCargoTypes.BattleCruiser:
                                {
                                    sc = new CargoShip(galaxyIDManager.PopFreeID(), 666, ShipStats[ShipTypes.BattleCruiser]);
                                    break;
                                }
                            case StatefulCargoTypes.Penguin:
                                {
                                    sc = new CargoShip(galaxyIDManager.PopFreeID(), 666, ShipStats[ShipTypes.Penguin]);
                                    break;
                                }
                            case StatefulCargoTypes.Reaper:
                                {
                                    sc = new CargoShip(galaxyIDManager.PopFreeID(), 666, ShipStats[ShipTypes.Reaper]);
                                    break;
                                }
                            case StatefulCargoTypes.LaserTurret:
                                {
                                    sc = new CargoLaserTurret(galaxyIDManager.PopFreeID(), 666, new LaserWeaponStats());
                                    break;
                                }
                            default:
                                {
                                    sc = new StatefulCargo(galaxyIDManager.PopFreeID(), s.Key);
                                    break;
                                }

                        }


                        CargoTransaction tr = new TransactionAddStatefulCargo(port, sc, true);
                        cargoSynchronizer.RequestTransaction(tr);
                        lastTransaction = tr;
                    }
                }

                foreach(var s in _config.PortConfig.StatelessCargoCounts)
                {
                    var tr = new TransactionAddStatelessCargo(port, s.Key, s.Value, true);
                    cargoSynchronizer.RequestTransaction(tr);
                    lastTransaction = tr;
                }

                foreach(var s in _config.PortConfig.ModuleCounts)
                {
                    Module m = ModuleFactory.CreateModule(s.Key, galaxyIDManager.PopFreeID(), 1);
                    var tr = new TransactionAddStatefulCargo(port, m, true);
                    cargoSynchronizer.RequestTransaction(tr);
                    lastTransaction = tr;
                }
                
            }
            if (lastTransaction != null)
            {                
                await lastTransaction.ResultTask;
            }
        }

        /// <summary>
        /// Creates a bunch of accounts
        /// </summary>
        IEnumerable<Account> CreateAccounts(AccountManager_MasterServer am)
        {
            List<Account> accounts = new List<Account>();

            accounts = new List<Account>();

            var userInfo = ParseMockUserData(_config.UserdataTextFilepath, _config.NumPlayers);
            int numAdded = 0;
         
            foreach(var ac in userInfo)
            {
                var res = am.CreateAccountAsync(ac.Username, ac.Password).Result;

                if(res.Item2 != 0)
                    throw new Exception("Account creation failed: " + res.Item2.ToString());

                accounts.Add(res.Item1);
                accounts[numAdded].IsAdmin = ac.IsAdmin;

                numAdded++;
            }

            return accounts;

        }

        /// <summary>
        /// receivingPlayers get mines in cargo, owningPlayers get the mines added to the receivingSystems (deployed in space) 
        /// </summary>
        /// <param name="receivingPlayers"></param>
        /// <param name="receivingSystems"></param>
        /// <param name="owningPlayers"></param>
        void AddMines(IEnumerable<Player> receivingPlayers, IEnumerable<PSystem> receivingSystems, IEnumerable<Player> owningPlayers, CargoSynchronizer cargoSynchronizer, GalaxyManager galaxyManager, LocalIDManager galaxyIDManager, StructureManager structureManager, LocatorService locatorService)
        {

            foreach (Player p in receivingPlayers)
            {
                for (int i = 0; i < _config.CARGO_NumMines; i++)
                {
                    TransactionAddStatefulCargo t = new TransactionAddStatefulCargo(p.GetActiveShip(), new StatefulCargo(galaxyIDManager.PopFreeID(), StatefulCargoTypes.DefensiveMine), true);
                    cargoSynchronizer.RequestTransaction(t);
                }
            }


            if (owningPlayers.Count() < 2)
                return;

            var itr = new CyclicalIterator<Player>(owningPlayers);
            itr.MoveNext();

            foreach (var system in galaxyManager.Systems)
            {
                for (int i = 0; i < _config.NumMinesPerSystem; i++)
                {
                    int ownerID = itr.Current.Id;
                    var mine = new DefensiveMine(Rand.Random.Next(-system.AreaSize/100, system.AreaSize/100), Rand.Random.Next(-system.AreaSize/100, system.AreaSize/100), galaxyIDManager.PopFreeID(), ownerID, system.Id, locatorService.PlayerLocator);
                    structureManager.RegisterObject(mine);
                    system.AddStructure(mine);

                    itr.MoveNext();
                }
            }



        }
        
        /// <summary>
        /// Creates a player for each account
        /// </summary>
        IEnumerable<Player> CreateHumanPlayers(IEnumerable<Account> accounts, PlayerManager pm, LocatorService locatorService)
        {
            var players = new List<Player>();

            foreach(var a in accounts)
            {               
                players.Add(pm.CreateHumanPlayer(a.Username, a, locatorService));
                a.PlayerID = players[players.Count - 1].Id;   
            }

            return players;
        }

        


        /// <summary>
        /// Gives each player a ship
        /// Galaxy must already have been generated
        /// </summary>
        async Task CreateShips(IEnumerable<Player> players, GalaxyManager galaxyManager, LocalIDManager IDManager, GalaxyRegistrationManager rm, PlayerManager pm, LocatorService locatorService)
        {
            ships = new List<IShip>();
            int counter = 0;
            foreach (Player p in players)
            {
                List<WeaponTypes> weapons = new List<WeaponTypes>
                {
                    WeaponTypes.PlasmaCannon,
                    WeaponTypes.BC_Laser,
                    WeaponTypes.LaserWave,
                    WeaponTypes.GravBomber,
                    WeaponTypes.HurrDurr

                };

                ShipCreationProperties props = new ShipCreationProperties(counter + 10, counter + 10, (int)galaxyManager.SolAreaID, PilotTypes.Player, ShipTypes.BattleCruiser, weapons );
                var tempShip = _dbFillerUtils.ShipFactory.CreateShip(props);
                tempShip.SetPlayer(p);
                p.SetActiveShip(tempShip, MockServer.WarpManager);                
                tempShip.CurrentEnergy = tempShip.ShipStats.Energy;
 

                ships.Add(tempShip);                
                counter++;
            }
        }

        async Task AddCargoToPlayerShips(IEnumerable<IShip> ships, ILocalIDManager galaxyIDManager, GalaxyRegistrationManager registrationManager, CargoSynchronizer cargoSynchronizer)
        {
            //Making this into a grand test of transaction sequences, there's really no reason to put this all into one sequence
            CargoTransactionSequence cs = new CargoTransactionSequence();

            foreach (var s in ships)
            {                
                for (int i = 0; i < _config.CARGO_NumTurrets; i++)//Sending ship state over a network might be painful while this is here...
                {

                    CargoLaserTurret c = new CargoLaserTurret(galaxyIDManager.PopFreeID(), 666, new LaserWeaponStats());
                    TransactionAddStatefulCargo t = new TransactionAddStatefulCargo(s, c, true);
                    cs.Add(t);
                                                           
                    registrationManager.RegisterObject(c);

                }

                TransactionAddStatelessCargo tr = new TransactionAddStatelessCargo(s, StatelessCargoTypes.AmbassadorMissile, _config.CARGO_NumMissiles, true);
                cs.Add(tr);

                tr = new TransactionAddStatelessCargo(s, StatelessCargoTypes.HellHoundMissile, _config.CARGO_NumMissiles, true);
                cs.Add(tr);
                tr = new TransactionAddStatelessCargo(s, StatelessCargoTypes.MissileType1, _config.CARGO_NumMissiles, true);
                cs.Add(tr);
                tr = new TransactionAddStatelessCargo(s, StatelessCargoTypes.MissileType2, _config.CARGO_NumMissiles, true);
                cs.Add(tr);
                tr = new TransactionAddStatelessCargo(s, StatelessCargoTypes.MissileType3, _config.CARGO_NumMissiles, true);
                cs.Add(tr);
                tr = new TransactionAddStatelessCargo(s, StatelessCargoTypes.MissileType4, _config.CARGO_NumMissiles, true);
                cs.Add(tr);

                tr = new TransactionAddStatelessCargo(s, StatelessCargoTypes.Biodome, _config.CARGO_NumBiodomes, true);
                cs.Add(tr);

                
            }
            cargoSynchronizer.RequestAtomicTransactionSequence(cs);
            await cs.ResultTask;
            if(cs.ResultTask.Result != CargoResult.Success)
            {
                ConsoleManager.WriteLine(cs.ResultTask.Result.ToString());
            }
            return;
        }

        /// <summary>
        /// Returns created NPCPlayers
        /// </summary>
        /// <param name="galaxyManager"></param>
        /// <param name="IDManager"></param>
        /// <param name="rm"></param>
        /// <param name="pm"></param>
        /// <param name="npcShips"></param>
        /// <returns></returns>
        async Task<IEnumerable<NPCPlayer>> CreateNPCs(GalaxyManager galaxyManager, LocalIDManager IDManager, GalaxyRegistrationManager rm, PlayerManager pm, LocatorService locatorService, CargoSynchronizer cargoSynchronizer, GlobalTeamManager teamManager, LocalIDManager galaxyIDManager)
        {
            Random r = new Random(666);

       
            var players = new List<NPCPlayer>();


            var systems = galaxyManager.Systems;
            int npcCount = 0;
            foreach(PSystem s in systems)
            {
                List<Player> team1 = new List<Player>();
                List<Player> team2 = new List<Player>();
                List<Player> team3 = new List<Player>();
                for (int i = 0; i < _config.NumNPCsPerSystem; i++)
                { 

                    List<WeaponTypes> weapons = new List<WeaponTypes>();

                    ShipTypes shipType = ShipTypes.Barge;
                    switch (npcCount % 3)
                    {
                        case 0:
                            shipType = ShipTypes.Penguin;
                            break;

                        case 1:
                            shipType = ShipTypes.Barge;
                            break;

                        case 2:
                            shipType = ShipTypes.Reaper;
                            break;
                    }

                    if (shipType == ShipTypes.Reaper)
                    {
                        weapons.Add(WeaponTypes.LaserWave);
                        weapons.Add(WeaponTypes.PlasmaCannon);
                    }
                    else
                    {
                        weapons.Add(WeaponTypes.AltLaser);
                        weapons.Add(WeaponTypes.LaserWave);
                    }

                    ShipCreationProperties props = new ShipCreationProperties(r.Next(-20, 20), r.Next(-20, 20), (int)galaxyManager.SolAreaID, PilotTypes.NPC, shipType, weapons);
                    IShip tempShip = _dbFillerUtils.ShipFactory.CreateShip(props);
                    tempShip.ShipStats.ShieldType = ShieldTypes.QuickRegen;

                    NPCPlayer p = pm.CreateNPCPlayer(locatorService);
                    pm.RegisterPlayer(p);
                    players.Add(p);

                    tempShip.SetPlayer(p);
                    p.SetActiveShip(tempShip, MockServer.WarpManager);
                    

                    TransactionAddStatelessCargo tr = new TransactionAddStatelessCargo(tempShip,
                        StatelessCargoTypes.AmbassadorMissile, 666666, true);
                    cargoSynchronizer.RequestTransaction(tr);
                    await tr.ResultTask;

                    tr = new TransactionAddStatelessCargo(tempShip, StatelessCargoTypes.Biodome, 666666, true);
                    cargoSynchronizer.RequestTransaction(tr);
                    await tr.ResultTask;

                    Helpers.DebugWarp(s, p, tempShip);

                    ships.Add(tempShip);

                    //Random team assignment
                    switch (npcCount%2)
                    {
                        case 0:
                            team1.Add(p);
                            break;
                        case 1:
                            team2.Add(p);
                            break;
                        case 2:
                            team3.Add(p);
                            break;
                    }


                    //Give the guy some turrets
                    for (int j = 0; j < _config.NumTurretsPerNPC; j++)
                    {
                        var t = StructureFactory.CreateStructure(StructureTypes.LaserTurret, r.Next(-20, 20),
                            r.Next(-20, 20), p, null, (int)p.CurrentAreaID, locatorService.PlayerLocator, true, dbm);

                        p.GetArea().AddStructure(t);
                    
                    }

                    AddModulesToShip(tempShip, 5, cargoSynchronizer, galaxyIDManager);

                    npcCount++;
                }

                foreach(Planet pl in s.GetPlanets())
                {
                    npcCount = 0;
                    for (int i = 0; i < _config.NumNpcsPerPlanet; i++)
                    {
                        ShipTypes shipType = ShipTypes.Barge;
                        switch (npcCount % 3)
                        {
                            case 0:
                                shipType = ShipTypes.Penguin;
                                break;

                            case 1:
                                shipType = ShipTypes.Barge;
                                break;

                            case 2:
                                shipType = ShipTypes.Reaper;
                                break;
                        }

                        NPCPlayer p = pm.CreateNPCPlayer(locatorService);
                        pm.RegisterPlayer(p);
                        players.Add(p);
                        IShip tempShip = new NPCShip(ShipStatManager.TypeToStats[shipType], locatorService);
                        tempShip.ShipStats.ShieldType = ShieldTypes.QuickRegen;
                        tempShip.Id = IDManager.PopFreeID();
                        rm.RegisterObject(tempShip);
                        p.SetActiveShip(tempShip, MockServer.WarpManager);


                        tempShip.SetWeapon(WeaponManager.GetNewWeapon(WeaponTypes.MissileLauncher));


                        if (shipType == ShipTypes.Reaper)
                        {
                            tempShip.SetWeapon(WeaponManager.GetNewWeapon(WeaponTypes.LaserWave));
                            tempShip.SetWeapon(WeaponManager.GetNewWeapon(WeaponTypes.PlasmaCannon));
                        }
                        else
                        {
                            tempShip.SetWeapon(WeaponManager.GetNewWeapon(WeaponTypes.AltLaser));
                            tempShip.SetWeapon(WeaponManager.GetNewWeapon(WeaponTypes.LaserWave));
                        }



                        TransactionAddStatelessCargo tr = new TransactionAddStatelessCargo(tempShip,
                            StatelessCargoTypes.AmbassadorMissile, 666666, true);
                        cargoSynchronizer.RequestTransaction(tr);
                        await tr.ResultTask;

                        tr = new TransactionAddStatelessCargo(tempShip, StatelessCargoTypes.Biodome, 666666, true);
                        cargoSynchronizer.RequestTransaction(tr);
                        await tr.ResultTask;

                        tempShip.PosX = r.Next(-20, 20);
                        tempShip.PosY = r.Next(-20, 20);

                        Helpers.DebugWarp(pl, p, tempShip);

                        
                        ships.Add(tempShip);

                        //Random team assignment
                        switch (npcCount % 2)
                        {
                            case 0:
                                team1.Add(p);
                                break;
                            case 1:
                                team2.Add(p);
                                break;
                            case 2:
                                team3.Add(p);
                                break;

                        }

                                               

                        AddModulesToShip(tempShip, 5, cargoSynchronizer, galaxyIDManager);

                        npcCount++;
                    }


                    teamManager.DebugCreateNewTeam(team1);
                    teamManager.DebugCreateNewTeam(team2);
                    teamManager.DebugCreateNewTeam(team3);
                }
            }

            return players;
           
        }

        async Task InsertShipStats(IDatabaseManager dbm)
        {
            ShipStats = CreateShipStats();


            foreach (var s in ShipStats)
            {
                await (dbm.SaveAsync(s.Value));
            }          


        }

        public Dictionary<ShipTypes,ShipStats> CreateShipStats()
        {
            var shipStats = new Dictionary<ShipTypes, ShipStats>();

            //Create some data
            shipStats.Add(ShipTypes.Penguin, new ShipStats
            {
                Name = "Penguin",
                Description = "I am a test penguin ship",
                MaxShields = 1000,
                MaxHealth = 10000,
                Energy = 1000,
                MaxHolds = 999999999,//Just enough for a couple turrets.
                Graphic = ShipTextures.Penguin,
                ThrustGraphic = "Basic",
                TopSpeed = 2.8f,
                BaseThrustForward = 200,
                BaseThrustReverse = 160,
                BaseThrustLateral = 200,
                TurnRate = 3.8f,
                Class = "Fighter",
                PlayerShip = true,
                ShipType = ShipTypes.Penguin,
                EnergyRegenRate = 0.3f,
                HaloShieldRegenRate = .333f,
                SlowShieldRegenRate = .03f,
                BaseWeight = 70,
                BoostBonus = 1.5f

            });
            shipStats.Add(ShipTypes.Reaper, new ShipStats
            {
                Name = "Jeyth",
                Description = "I am a test jeyth ship",
                MaxShields = 6000,
                MaxHealth = 12000,
                Energy = 1400,
                MaxHolds = 40,
                Graphic = ShipTextures.Reaper,
                ThrustGraphic = "Basic",
                TopSpeed = 3.0f,
                BaseThrustForward = 240,
                BaseThrustReverse = 192,
                BaseThrustLateral = 240,
                TurnRate = 4.0f,
                Class = "Fighter",
                PlayerShip = true,
                ShipType = ShipTypes.Reaper,
                EnergyRegenRate = 0.5f,
                HaloShieldRegenRate = .4f,
                SlowShieldRegenRate = .035f,
                BaseWeight = 70,
                BoostBonus = 1.5f
            });
            shipStats.Add(ShipTypes.Barge, new ShipStats
            {
                Name = "Barge",
                Description = "I am a test barge ship",
                MaxShields = 8000,
                MaxHealth = 8000,
                Energy = 600,
                MaxHolds = 200,
                Graphic = ShipTextures.ZYVariantBarge,
                ThrustGraphic = "Basic",
                TopSpeed = 2.0f,
                BaseThrustForward = 140,
                BaseThrustReverse = 112,
                BaseThrustLateral = 140,
                TurnRate = 2.0f,
                Class = "Freighter",
                PlayerShip = true,
                ShipType = ShipTypes.Barge,
                EnergyRegenRate = 0.25f,
                HaloShieldRegenRate = .25f,
                SlowShieldRegenRate = .02f,
                BaseWeight = 70,
                BoostBonus = 1.5f
            });
            shipStats.Add(ShipTypes.BattleCruiser, new ShipStats
            {
                Name = "Battlecruiser",
                Description = "I am a test barge ship",
                MaxShields = 1500,
                MaxHealth = 16000,
                Energy = 1500,
                MaxHolds = 999999999,//Just enough for a couple turrets.
                Graphic = ShipTextures.Battlecruiser,
                ThrustGraphic = "Basic",
                TopSpeed = 10f,
                BaseThrustForward = 240,
                BaseThrustReverse = 112,
                BaseThrustLateral = 140,
                TurnRate = 3.5f,
                Class = "Freighter",
                PlayerShip = true,
                ShipType = ShipTypes.BattleCruiser,
                EnergyRegenRate = 0.6f,
                HaloShieldRegenRate = .1f,
                SlowShieldRegenRate = .01f,
                BaseWeight = 100,
                BoostBonus = 5.5f
            });

            shipStats.Add(ShipTypes.SuperCoolAwesome3DShip, new ShipStats
            {
                Name = "SuperCoolAwesome3DShip",
                Description = "I am a test barge ship",
                MaxShields = 1500,
                MaxHealth = 16000,
                Energy = 1500,
                MaxHolds = 999999999,//Just enough for a couple turrets.
                Graphic = ShipTextures.None,
                ThrustGraphic = "Basic",
                TopSpeed = 10f,
                BaseThrustForward = 240,
                BaseThrustReverse = 112,
                BaseThrustLateral = 140,
                TurnRate = 3.5f,
                Class = "FUCKYOURSHITRIGHTUP",
                PlayerShip = true,
                ShipType = ShipTypes.SuperCoolAwesome3DShip,
                EnergyRegenRate = 0.6f,
                HaloShieldRegenRate = .1f,
                SlowShieldRegenRate = .01f,
                BaseWeight = 100,
                BoostBonus = 5.5f
            });

            return shipStats;

        }

        public void AddModulesToShip(IShip ship, int numMods, CargoSynchronizer cargoSynchronizer, LocalIDManager galaxyIDManager)
        {
            for (int i = 0; i < numMods; i++)
            {
                Module m = null;
                int moduleID = galaxyIDManager.PopFreeID();
                byte level = (byte)Rand.Random.Next(1, 4);
                int modnum = Rand.Random.Next(1, 11);
                {
                    switch (modnum)
                    {
                        case 1:
                            m = new EnergyRegenModule(moduleID, level);
                            break;
                        case 2:
                            m = new ThrustModule(moduleID, level);
                            break;
                        case 3:
                            m = new MaxShieldModule(moduleID, level);
                            break;
                        case 4:
                            m = new ShieldRegenModule(moduleID, level);
                            break;
                        case 5:
                            m = new MaxEnergyModule(moduleID, level);
                            break;
                        case 6:
                            m = new DamageModule(moduleID, level);
                            break;
                        case 7:
                            m = new DefenseModule(moduleID, level);
                            break;
                        case 8:
                            m = new TurnRateModule(moduleID, level);
                            break;
                        case 9:
                            m = new LateralThrustModule(moduleID, level);
                            break;
                        case 10:
                            m = new TopSpeedModule(moduleID, level);
                            break;
                    }
                }
                TransactionAddStatefulCargo str = new TransactionAddStatefulCargo(ship, m, true);
                str.OnCompletion += ship.CargoAdded;
                cargoSynchronizer.RequestTransaction(str);
            }

        }

        IEnumerable<AccountData> ParseMockUserData(string filename, int numUsers)
        {
            var dict = new List<AccountData>();
            var uniqueCheck = new HashSet<string>();

            var allLines = File.ReadAllLines(filename);

            if(allLines.Length < numUsers)
                throw new InvalidOperationException("Not enough users read from " + filename + ". Either change the number of users in DBFillerConfig or add users to the file");

            for (int i = 0; i < numUsers; i++)
            {
                var cl = allLines[i].Split();
                var ac = new AccountData
                {
                    Username = cl[0],
                    Password = cl[1],
                    IsAdmin = bool.Parse(cl[2])
                };

                
                if(uniqueCheck.Contains(ac.Username))
                    throw new InvalidOperationException("Nonunique username detected in " + filename);
                dict.Add(ac);

                uniqueCheck.Add(ac.Username);

            }

            return dict;

        }
        
         
    }

    internal class AccountData
    {
        public string Username;
        public string Password;
        public bool IsAdmin;
    }
}
