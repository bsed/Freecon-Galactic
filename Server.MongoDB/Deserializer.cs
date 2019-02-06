using System;
using Server.Database;
using Server.Models;
using Server.Models.Interfaces;
using Server.Models.Structures;
using SRServer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Freecon.Models.TypeEnums;
using Server.Interfaces;
using RedisWrapper;
using Core;

namespace Server.MongoDB
{
    /// <summary>
    /// Helper class for deserializing different models
    /// </summary>
    public class Deserializer
    {
        /// <summary>
        /// Instantiates and registers all objects associated with a PSystem (planets, players, ships, etc)
        /// </summary>
        public static async Task<PSystem> DeserializePSystemAsync(PSystemModel system, RedisServer redisServer, LocatorService ls, IGalaxyRegistrationManager rm, IDatabaseManager dbm)
        {
            PSystem retSys = new PSystem(system, ls);
            List<IShip> deserializedShips = new List<IShip>();
            List<IStructure> loadedStructures = new List<IStructure>();
            List<Player> loadedPlayers = new List<Player>();

            //For now we just run the whole lambda synchronously in its own thread. Marking it async makes this method return complete before the task is complete, need to investigate...
            await Task.Factory.StartNew(() =>
                {
                    

                    HashSet<int> colonyIDsToLoad = new HashSet<int>();
                    HashSet<int> shipIDsToLoad = new HashSet<int>();
                    HashSet<int> areaIDsToLoad = new HashSet<int>();
                    List<int> structureIDsToLoad = new List<int>();
                    HashSet<int> layoutIDsToLoad = new HashSet<int>();

                    foreach (int id in system.ShipIDs)
                    {
                        if(shipIDsToLoad.Contains(id))
                        {
                            throw new CorruptStateException("Multiple areas contain the same shipID.");
                        }
                        shipIDsToLoad.Add(id);
                    }
                    foreach (var id in retSys.MoonIDs)
                        areaIDsToLoad.Add(id);

                    foreach (var id in retSys.PlanetIDs)
                        areaIDsToLoad.Add(id);

                    foreach (var id in retSys.PortIDs)
                        areaIDsToLoad.Add(id);
                    
                    IEnumerable<AreaModel> loadedAreaModels = dbm.GetAreasAsync(areaIDsToLoad).Result;

                    foreach (var am in loadedAreaModels)
                    {
                        switch (am.AreaType)
                        {
                            case AreaTypes.Planet:
                                layoutIDsToLoad.Add(((PlanetModel)am).LayoutId);
                                break;
                        }
                    }

                   
                    IEnumerable<PlanetLayout> loadedLayouts = dbm.GetLayoutsAsync(layoutIDsToLoad).Result.Select(s=>(PlanetLayout)s);
                    Dictionary<int, PlanetLayout> layouts = new Dictionary<int, PlanetLayout>();
                    foreach (var l in loadedLayouts)
                    {
                        layouts.Add(l.Id, l);
                    }


                    var loadedAreas = new List<IArea>();
                    // Instantiate all areas
                    foreach (AreaModel am in loadedAreaModels)
                    {
                        IArea loadedArea = null;
                        structureIDsToLoad.AddRange(am.StructureIDs);

                        // Planets
                        if(am.AreaType == AreaTypes.Planet)
                        {
                            loadedArea = new Planet((PlanetModel)am, layouts[((PlanetModel)am).LayoutId], ls);
                            var p = loadedArea as Planet;
                            //rm.RegisterObject(p);
                            if (p.ColonyID != null)
                            {
                                colonyIDsToLoad.Add((int)p.ColonyID);
                            }
                        }

                        // Ports
                        else if(am.AreaType == AreaTypes.Port)
                        {
                            loadedArea = new Port((PortModel)am, ls);
                        }
                        else
                        {
                            throw new Exception("Error: Loaded area not handled in DeserializePSystem()");
                        }

                        

                        foreach (var id in am.ShipIDs)
                        {
                            if (shipIDsToLoad.Contains(id))
                            {
                                throw new CorruptStateException("Multiple areas contain the same shipID.");
                            }
                            shipIDsToLoad.Add(id);                           
                        }
                        loadedAreas.Add(loadedArea);
                        rm.RegisterObject(loadedArea);

                    }

                    // Colonies
                    IEnumerable<AreaModel> LoadedColonies = dbm.GetAreasAsync(colonyIDsToLoad).Result;
                    List<Colony> deserializedColonies = new List<Colony>();
                    foreach (AreaModel am in LoadedColonies)
                    {
                        if (am.AreaType == AreaTypes.Colony)
                        {
                            Colony c = new Colony((ColonyModel)am, ls);
                            c.DisableUpdates = true;
                            rm.RegisterObject(c);
                            deserializedColonies.Add(c);
                        }
                        else
                        {
                            throw new Exception("AreaID query resulted in an AreaModel which was not a ColonyModel in DeserializePSystem()");
                        }
                        foreach (var id in am.ShipIDs)
                        {
                            if (shipIDsToLoad.Contains(id))
                            {
                                throw new CorruptStateException("Multiple areas contain the same shipID.");
                            }
                            shipIDsToLoad.Add(id);
                        }
                    }

                    // Structures
                    loadedStructures.AddRange(LoadStructures(retSys, dbm, rm, ls.PlayerLocator).Result);
                    
                    foreach(IArea loadedArea in loadedAreas)
                    {
                        if (loadedArea is IHasStructures)
                        {                            
                            loadedStructures.AddRange(LoadStructures((IHasStructures)loadedArea, dbm, rm, ls.PlayerLocator).Result);                                                    
                        }

                    }
                    

                    // Ships
                    IEnumerable<ShipModel> loadedShipModels = dbm.GetShipsAsync(shipIDsToLoad).Result;
                    HashSet<int> playerIDsToLoad = new HashSet<int>();
                    foreach (var s in loadedShipModels)
                    {
                        var loadedShip = DeserializeShip(s, ls, rm);
                        deserializedShips.Add(loadedShip);
                        if(loadedShip.PlayerID != null)
                        {
                            playerIDsToLoad.Add((int)s.PlayerID);
                        }
                        
                    }


                    // Players
                    IEnumerable<PlayerModel> loadedPlayerModels = dbm.GetPlayersAsync(playerIDsToLoad).Result;
                    
                    HashSet<int> accountIDsToLoad = new HashSet<int>();
                    foreach (var p in loadedPlayerModels)
                    {
                        if (p.PlayerType == PlayerTypes.Human)
                        {
                            HumanPlayer hp = new HumanPlayer(p, ls);
                            rm.RegisterObject(hp);
                            loadedPlayers.Add(hp);
                        }
                        else
                        {
                            NPCPlayer np = new NPCPlayer(p, ls);
                            np.MessageService = new RedisOutgoingMessageService(redisServer, np);
                            rm.RegisterObject(np);
                            loadedPlayers.Add(np);
                          }
                        if (p.AccountID != null)
                        {
                            accountIDsToLoad.Add((int)p.AccountID);
                        }
                    }                   
                    

                    //Accounts
                    IEnumerable<AccountModel> loadedAccounts = dbm.GetAccountsAsync(accountIDsToLoad).Result;

                    foreach(var a in loadedAccounts)
                    {
                        rm.RegisterObject(new Account(a));
                    }

                    foreach(var c in deserializedColonies)
                    {
                        c.DisableUpdates = false;
                    }
                }
                );
            rm.RegisterObject(retSys);

            foreach (Player p in loadedPlayers)
            {
                if (p.PlayerType == PlayerTypes.NPC)
                {
                    p.GetArea().MovePlayerHere(p, false);

                }
            }

            foreach(var s in deserializedShips)
            {
                IArea a = s.GetArea();

                if(a == null)//Corrupt state, the ship's CurrentAreaId doesn't match the list is for an area which hasn't been loaded yet. Abort the ship read.
                {
                    //I'm not sure that there's a way to gracefully handle this without a major rewrite and a bunch of overhead, so it'll be an exception for now.
                    throw new CorruptStateException("Ship's CurrentAreaId does not match the area from which it was loaded. Expect exceptions.");

                }
                else
                {
                    a.AddShip(s, true);
                }               
                            
            }


            return retSys;

        }

        public static IShip DeserializeShip(ShipModel s, LocatorService ls, IGalaxyRegistrationManager rm)
        {
            IShip retShip;

            if(s.PilotType == PilotTypes.Player)
            {
                retShip = new PlayerShip((PlayerShipModel)s, ls);
                rm.RegisterObject(retShip);
            }
            else if(s.PilotType == PilotTypes.NPC)
            {
                retShip = new NPCShip((NPCShipModel)s, ls);
                rm.RegisterObject(retShip);
            }
            else
            {
                throw new Exception("Deserialization not implemented for PilotTypes " + s.PilotType + ".");
            }
                        

            return retShip;
            
        }
        
        static async Task InstantiateStructures(IEnumerable<IArea> loadedAreas, Dictionary<int, IStructureModel> loadedStructureModels, IGalaxyRegistrationManager rm, IPlayerLocator pl)
        {

            foreach(IArea a in loadedAreas)
            {
                var structureIDs = a.GetStructureIDs();
                foreach(int id in structureIDs)
                {
                    IStructure s = StructureHelper.InstantiateStructure(loadedStructureModels[id], pl, rm);
                    a.AddStructure(s);
                }

            }
        }

      

        /// <summary>
        /// Loads the structures associated with the given object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static async Task<List<IStructure>> LoadStructures(IHasStructures loadMine, IDatabaseManager dbm, IGalaxyRegistrationManager rm, IPlayerLocator pl)
        {           
            var loadedStructureModels = await dbm.GetStructuresAsync(loadMine.GetStructureIDs());
            List<IStructure> loadedStructures = new List<IStructure>();
            foreach(var ls in loadedStructureModels)
            {
                IStructure s = StructureHelper.InstantiateStructure(ls, pl, rm);
                loadMine.AddStructure(s);
                loadedStructures.Add(s);
            }

            return loadedStructures;
        }
    }
}
