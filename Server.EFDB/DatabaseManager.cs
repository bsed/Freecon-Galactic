using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Models;
using Server.Models.Interfaces;
using System.Data.Entity;
using Server.EFDB;
using Server.EFDB.Models;
using Server.Interfaces;
using Freecon.Models.TypeEnums;
using System.Data.Entity.Migrations;
using SRServer.Services;
using Server.Models.Database;
using SRServer.Models.Interfaces;
using Core.Logging;
using Server.Models.Structures;

namespace Server.Database
{
    public class DatabaseManager : IDBWriter
    {
        GalacticObjectContext _galacticObjectContext;
        Dictionary<Type, DbSet<IDBObject>> _typeToDBSet;

        public SimpleLogger Logger;

        /// <summary>
        /// We should probably remove deleteExistingDB before release. Implemented for DBFiller.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="deleteExistingDB"></param>
        public DatabaseManager(SimpleLogger logger = null, bool deleteExistingDB = false)
        {
            _galacticObjectContext = new GalacticObjectContext(deleteExistingDB);
            
            _registerDBSets();

            Logger = logger;
        }

        void _registerDBSets()
        {
            //Doesn't work, because .AddOrUpdate extension is only defined for IDbSet<type>

            //_typeToDBSet = new Dictionary<Type, DbSet<IDBObject>>();
            //_typeToDBSet.Add(typeof(DBShip), _galacticObjectContext.Ships);

            ////Areas
            //_typeToDBSet.Add(typeof(DBArea), _galacticObjectContext.Areas);
            //_typeToDBSet.Add(typeof(Moon), _galacticObjectContext.Areas);
            //_typeToDBSet.Add(typeof(Planet), _galacticObjectContext.Areas);
            //_typeToDBSet.Add(typeof(DBPlayer), _galacticObjectContext.Players);
            //_typeToDBSet.Add(typeof(DBAccount), _galacticObjectContext.Accounts);
            //_typeToDBSet.Add(typeof(PlanetLayout), _galacticObjectContext.Layouts);
            //_typeToDBSet.Add(typeof(ShipStats), _galacticObjectContext.ShipStats);

        }
        
        /// <summary>
        /// Returns null if ship is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Ship GetShip(int id, IPlayerLocator pl, IAreaLocator al, ITeamLocator tl)
        {

            
            //Include is important, requires EF to load specified related entities, which otherwise aren't loaded with the DBShip, because of performance (fewer joins). The alternative is lazy loading with virtual, but this is way cooler bro.
            //Probably should use virtuals in the future, though.
            DBShip s = _galacticObjectContext.Ships.Include(e => e.PrimaryWeapon).Include(x => x.SecondaryWeapon).Include(y => y.MissileLauncher).First( ls => ls.Id == id );
            if (s == null)
                return null;
            else
                switch(s.PilotType)
                {
                    case PilotTypes.Player:
                        return new PlayerShip((DBPlayerShip)s, pl, al, tl);
                    default:
                        throw new Exception("Error: Deserialization not implemented for ship of PilotType " + s.PilotType);  


                }

           
        }

        public IEnumerable<Ship> GetAllShips(IPlayerLocator pl, IAreaLocator al, ITeamLocator tl)
        {
            IEnumerable<DBShip> allShips = _galacticObjectContext.Ships;

            List<Ship> acc = new List<Ship>();
            foreach (DBShip s in allShips)
            {
                switch (s.PilotType)
                {
                    case PilotTypes.Player:
                        acc.Add(new PlayerShip((DBPlayerShip)s, pl, al, tl));
                        break;
                    default:
                        throw new Exception("Error: Deserialization not implemented for ship of PilotType " + s.PilotType);


                }
            }

            return acc;

        }

        /// <summary>
        /// Returns null if area is not found. Will probably be depreciated soon, because of excessive querying.
        /// Note that this method does not load nested references if they are stored as Ids (e.g. planets in systems, which are stored only by IDs: PSystem.PlanetIDs) 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Area GetArea(int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm, IGalaxyRegistrationManager rm, ITeamLocator tl)
        {
            //Apparently, include is retardedly slow, so we have to use a manual workaround.
            //DBArea a = _galacticObjectContext.Areas.Include(e=>e.Warpholes).Include(se=>se.Structures).First(ee=>ee.Id == id);
            Area retArea;
            using (GalacticObjectContext context = new GalacticObjectContext())
            {
                context.SetLogger(Logger);

                if(context.Systems.Any(e=>e.Id == id))
                {
                    retArea = GetSystem(context, id, pl, al, sl, mm);
                }
                else if (context.Moons.Any(e => e.Id == id))
                {
                    retArea = GetMoon(context, id, pl, al, sl, mm);
                }
                else if (context.Planets.Any(e => e.Id == id))
                {
                    retArea = GetPlanet(context, id, pl, al, sl, mm, rm, tl);
                }
                else if (context.Ports.Any(e => e.Id == id))
                {
                    retArea = GetPort(context, id, pl, al, sl, mm);
                }
                else if (context.Colonies.Any(e => e.Id == id))
                {
                    retArea = GetColony(context, id, pl, al, sl, mm);
                }
                else
                    throw new Exception("Error: Area not found. Has deserialization been imlemented for this type?");
                
             
                List<IHasGalaxyID> l = new List<IHasGalaxyID>();
                retArea.GetRegisterableNestedObjects(l);
                foreach (IHasGalaxyID obj in l)
                    rm.RegisterObject(obj);

                
            }
            return retArea;

        }

        public Planet GetPlanet(GalacticObjectContext context, int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm, IGalaxyRegistrationManager rm, ITeamLocator tl)
        {
            var a = context.Planets.Include(e0 => e0.Warpholes).Include(e1 => e1.Structures).Include(e2 => e2.Colony).First(e3 => e3.Id == id);

            if (a != null)
            {
                Planet retArea = new Planet(a, pl, al, sl, mm, tl);
                if (a.Colony != null)
                {
                    Colony col = new Colony(a.Colony, pl, al, sl, mm);
                    rm.RegisterObject(col);

                    foreach (var s in retArea.GetStructures())
                        col.RegisterStructure(s.Value);
                }
            }


            return a == null ? null : new Planet(a, pl, al, sl, mm, tl);
        }
        
        public Moon GetMoon(GalacticObjectContext context,int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm)
        {
            var a = context.Moons.Include(e0 => e0.Warpholes).Include(e1 => e1.Structures).First(e2 => e2.Id == id);
            return a == null ? null : new Moon(a, pl, al, sl, mm);
        }
        
        public PSystem GetSystem(GalacticObjectContext context,int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm)
        {
            var a = context.Systems.Include(e0 => e0.Warpholes).First(e2 => e2.Id == id);
            return a == null ? null : new PSystem(a, pl, al, sl, mm);
        }
        
        public Port GetPort(GalacticObjectContext context,int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm)
        {
            var a = context.Ports.Find(id);
            return a == null ? null : new Port(a, pl, al, sl, mm);
        }
        
        public Colony GetColony(GalacticObjectContext context,int id, IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm)
        {
            var a = context.Colonies.First(e3=>e3.Id == id);
            return a == null ? null : new Colony(a, pl, al, sl, mm);
        }

        /// <summary>
        /// Does not load or register nested objects within systems (e.g. planets)
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="al"></param>
        /// <param name="sl"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        public IEnumerable<PSystem> GetAllSystems(IPlayerLocator pl, IAreaLocator al, IShipLocator sl, IMessageManager mm)
        {
            IEnumerable<DBArea> allAreas = _galacticObjectContext.Systems;

            List<PSystem> acc = new List<PSystem>();
            foreach (DBArea a in allAreas)
            {
                switch (a.Type)
                {
                    case AreaTypes.System:
                        acc.Add(new PSystem((DBPSystem)a, pl, al, sl, mm));
                        break;                   
                }
            }

            return acc;

        }

        /// <summary>
        /// Returns null if player not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Player GetPlayer(int id, IAreaLocator al, IShipLocator sl, IAccountLocator acl, ITeamLocator tl, ITeamManager tm)
        {
            DBPlayer p = _galacticObjectContext.Players.First(ee=>ee.Id == id); 
            return p != null ? new Player(p, al, sl, acl,tl, tm) : null;     
        }

        public IEnumerable<Player> GetAllPlayers(IAreaLocator al, IShipLocator sl, IAccountLocator acl, ITeamLocator tl, ITeamManager tm)
        {
            IEnumerable<DBPlayer> allPlayers = _galacticObjectContext.Players;

            List<Player> acc = new List<Player>();
            foreach (DBPlayer p in allPlayers)
            {
                acc.Add(new Player(p, al, sl, acl, tl, tm));
            }

            return acc;

        }


        /// <summary>
        /// Returns null if account not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Account GetAccount(int id)
        {            
            DBAccount a = _galacticObjectContext.Accounts.Find(id);
            return a != null ? new Account(a) : null;
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            IEnumerable<DBAccount> allAccounts = _galacticObjectContext.Accounts;
            
            List<Account> acc = new List<Account>();
            foreach (DBAccount a in allAccounts)
            {
                acc.Add(new Account(a));
            }

            return acc;

        }

        public Team GetTeam(int id)
        {
            DBTeam t = _galacticObjectContext.Teams.Find(id);
            return t != null ? new Team(t) : null;

        }

        public void DeleteTeam(int id)
        {
            DBTeam t = new DBTeam();
            t.Id = id;
            _galacticObjectContext.Teams.Attach(t);
            _galacticObjectContext.Teams.Remove(t);


        }

        public IEnumerable<ShipStats> GetStatsFromDB()
        {
            return _galacticObjectContext.ShipStats;
        }

        /// <summary>
        /// Adds the object to the database and the db context to automatically track changes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveChanges">Force db write. Set to false when sequentially inserting multiple objects. Manual context.SaveChanges required after.</param>
        /// <returns></returns>
        public IEFSerializable Add(IEFSerializable obj, bool saveChanges = true)
        {
            return Add(obj, _galacticObjectContext, saveChanges);
        }

        IEFSerializable Add(IEFSerializable obj, GalacticObjectContext c, bool saveChanges = true)
        {

            IDBObject saveme = obj.GetDBObject();
                //_getDBSet(saveme).Add(saveme);
            Type t = saveme.GetType();

                //This could use a convenient workaround...

            if (t == typeof(DBPSystem))
                c.Systems.AddOrUpdate((DBPSystem)saveme);
            else if (t == typeof(DBMoon))
                c.Moons.AddOrUpdate((DBMoon)saveme);
            else if (t == typeof(DBPort))
                c.Ports.AddOrUpdate((DBPort)saveme);
            else if (t == typeof(DBPlanet))
                c.Planets.AddOrUpdate((DBPlanet)saveme);
            else if (t == typeof(DBColony))
                c.Colonies.AddOrUpdate((DBColony)saveme);
            else if (t == typeof(DBShip) || t.IsSubclassOf(typeof(DBShip)))
                c.Ships.AddOrUpdate((DBShip)saveme);
            else if (t == typeof(DBPlayer) || t.IsSubclassOf(typeof(DBPlayer)))
                c.Players.AddOrUpdate((DBPlayer)saveme);
            else if (t == typeof(DBAccount) || t.IsSubclassOf(typeof(DBAccount)))
                c.Accounts.AddOrUpdate((DBAccount)saveme);
            else if (t == typeof(PlanetLayout) || t.IsSubclassOf(typeof(PlanetLayout)))
                c.Layouts.AddOrUpdate((PlanetLayout)saveme);
            else if (t == typeof(ShipStats) || t.IsSubclassOf(typeof(ShipStats)))
                c.ShipStats.AddOrUpdate((ShipStats)saveme);
            else if (t == typeof(DBTeam))
                c.Teams.AddOrUpdate((DBTeam)saveme);
            else if (t == typeof(StructureModel) || t.IsSubclassOf(typeof(StructureModel)))
                c.Structures.Add((StructureModel)saveme);
            else
                throw new Exception("Error: Entity set not available for objects of type " + t.ToString());




            if (saveChanges)
                c.SaveChanges();
        
            return obj;
        }
        
        public IEnumerable<IEFSerializable> Add(IEnumerable<IEFSerializable> objects)
        {
            

            using (GalacticObjectContext c = new GalacticObjectContext())
            {
                c.Configuration.AutoDetectChangesEnabled = false;

                foreach (var o in objects)
                    Add(o, c, false);
                c.SaveChanges();
            }
            
            
            return objects;
        }

        public void Test()
        {
            CommandCenterModel cm = new CommandCenterModel();
            cm.OwnerID = 5;
            cm.Id = 231214;
            cm.OwnerTeamID = 66;

            _galacticObjectContext.CommandCenters.Add(cm);
            _galacticObjectContext = new GalacticObjectContext();


        }

        /// <summary>
        /// Deletes an object from the db
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="saveChanges">Force db write. Set to false when sequentially inserting multiple objects. Manual context.SaveChanges required after.</param>
        /// <returns></returns>
        public IEFSerializable Delete(IEFSerializable obj, bool saveChanges = true)
        {
            var tt = obj.GetDBObject();//TODO: This is needlessly slow, need to come back and fix later
            var t = obj.GetType();

            if (t == typeof(DBPSystem))
            {
                _galacticObjectContext.Systems.Attach((DBPSystem)tt);
                _galacticObjectContext.Systems.Remove((DBPSystem)tt);
            }
            else if (t == typeof(DBMoon))
            {
                _galacticObjectContext.Moons.Attach((DBMoon)tt);
                _galacticObjectContext.Moons.Remove((DBMoon)tt);
            }
            else if (t == typeof(DBPort))
            {
                _galacticObjectContext.Ports.Attach((DBPort)tt);
                _galacticObjectContext.Ports.Remove((DBPort)tt);
            }
            else if (t == typeof(DBPlanet))
            {
                _galacticObjectContext.Planets.Attach((DBPlanet)tt);
                _galacticObjectContext.Planets.Remove((DBPlanet)tt);
            }
            else if (t == typeof(DBColony))
            {
                _galacticObjectContext.Colonies.Attach((DBColony)tt);
                _galacticObjectContext.Colonies.Remove((DBColony)tt);
            }
            else if (t == typeof(DBShip) || t.IsSubclassOf(typeof(DBShip)))
            {
                _galacticObjectContext.Ships.Attach((DBShip)tt);
                _galacticObjectContext.Ships.Remove((DBShip)tt);
            }
            else if (t == typeof(DBPlayer) || t.IsSubclassOf(typeof(DBPlayer)))
            {
                _galacticObjectContext.Players.Attach((DBPlayer)tt);
                _galacticObjectContext.Players.Remove((DBPlayer)tt);
            }
            else if (t == typeof(DBAccount) || t.IsSubclassOf(typeof(DBAccount)))
            {
                _galacticObjectContext.Accounts.Attach((DBAccount)tt);
                _galacticObjectContext.Accounts.Remove((DBAccount)tt);
            }
            else if (t == typeof(PlanetLayout) || t.IsSubclassOf(typeof(PlanetLayout)))
            {
                _galacticObjectContext.Layouts.Attach((PlanetLayout)tt);
                _galacticObjectContext.Layouts.Remove((PlanetLayout)tt);
            }
            else if (t == typeof(ShipStats) || t.IsSubclassOf(typeof(ShipStats)))
            {
                _galacticObjectContext.ShipStats.Attach((ShipStats)tt);
                _galacticObjectContext.ShipStats.Remove((ShipStats)tt);
            }
            else if (t == typeof(DBTeam))
            {
                _galacticObjectContext.Teams.Attach((DBTeam)tt);
                _galacticObjectContext.Teams.Remove((DBTeam)tt);

            }
            else
                throw new Exception("Error: Entity set not available for objects of type " + t.ToString());



            //_getDBSet(t).Remove(t);

            if (saveChanges)
                _galacticObjectContext.SaveChanges();

            return obj;
        }

        public IEnumerable<IEFSerializable> Delete(IEnumerable<IEFSerializable> objects)
        {
            foreach (var o in objects)
                Delete(o, false);

            _galacticObjectContext.SaveChanges();
            return objects;
        }
     
        public IEnumerable<PlanetLayout> ReadLayouts()
        {
            return _galacticObjectContext.Layouts.Include(e=>e.Warpholes);
        }

        public void ResetContext()
        {
            _galacticObjectContext = new GalacticObjectContext();
            _galacticObjectContext.SetLogger(Logger);
           
        }
        ///// <summary>
        ///// Resolves the correct DBSet in which to save the object, based on the object's type
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //DbSet<DBArea> _getDBSet(IDBObject obj)
        //{
        //    //Broken
        //    //var t = (obj.GetType());//Original type

            
        //    //if (_typeToDBSet.ContainsKey(t))
        //    //{
        //    //    return _typeToDBSet[t];
        //    //}
        //    //else if ((obj.GetType().IsSubclassOf(typeof(DBArea))))
        //    //{
        //    //    return _galacticObjectContext.Areas;
        //    //}
        //    //else if ((obj.GetType().IsSubclassOf(typeof(DBShip))))
        //    //{
        //    //    return _galacticObjectContext.Ships;
        //    //}
        //    //else
        //    //{
        //    //    throw new Exception("Error: Serialization not defined for type " + obj.GetType().ToString());
        //    //}

        //}
                

 
       


        
        


#if DEBUG
        /// <summary>
        /// Probably shouldn't use this after release.
        /// </summary>
        public void ClearDatabase()
        {
            _galacticObjectContext.Database.Delete();

            //Ship below should be the name of the table, but I don't know what the table name is ATM. To be fixed.
            //_galacticObjectContext.Database.ExecuteSqlCommand("delete from Ship");

            //Slow way for now
            //_galacticObjectContext.Areas.RemoveRange(_galacticObjectContext.Areas);
            //_galacticObjectContext.Ships.RemoveRange(_galacticObjectContext.Ships);
            //_galacticObjectContext.Players.RemoveRange(_galacticObjectContext.Players);
            //_galacticObjectContext.Accounts.RemoveRange(_galacticObjectContext.Accounts);



            //_galacticObjectContext.SaveChanges();

        }



#endif
    }
}
