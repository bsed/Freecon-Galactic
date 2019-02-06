using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Freecon.Core.Interfaces;
using SRServer.Services;
using Freecon.Models.TypeEnums;
using Server.Models;
using Server.Models.Interfaces;
using Server.Models.Space;
using Server.Models.Structures;
using Server.Factories;
using Server.Interfaces;
using Server.Database;


namespace Server.Managers
{
    public class GalaxyManager:IAreaLocator
    {
        public ConcurrentDictionary<int, Port> AllPorts = new ConcurrentDictionary<int, Port>();

        /// <summary>
        /// Systems currently being handled by this slave
        /// </summary>
        public List<PSystem> Systems { get { return _localSystems.Values.ToList(); } }
        static ConcurrentDictionary<int, PSystem> _localSystems;//Systems currently being handled by this slave
        public int LocalSystemCount { get { return _localSystems.Count; } }
        public ConcurrentDictionary<int, PSystem> idToSystem; // Can be changed to reference to System

        //All uniqueIDed objects stored here (Systems, ships, turrets, etc)
        public ConcurrentDictionary<int, IHasGalaxyID> AllObjects = new ConcurrentDictionary<int, IHasGalaxyID>();

        public ConcurrentDictionary<int, ISimulatable> AllSimulatableObjects = new ConcurrentDictionary<int, ISimulatable>();

        ITeamLocator _teamLocator;

        public int NumberOfSystems;

        public ConcurrentDictionary<int, IArea> AllAreas; //All of the areas in the galaxy. Convenient for area lookups.
   
        public int? SolAreaID { get; private set; }    

        public GalaxyManager(int solID, ITeamLocator tl)
        {
            idToSystem = new ConcurrentDictionary<int, PSystem>();
            AllAreas = new ConcurrentDictionary<int, IArea>();
            
            _localSystems = new ConcurrentDictionary<int, PSystem>();          
           
            SolAreaID = solID;

            _teamLocator = tl;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentTime">Time, in milliseconds, since server start (stopwatch.ElapsedMilliseconds at time of writing)</param>
        public void UpdateAreas(float currentTime)
        {
            foreach (var kvp in AllAreas)
            {
                kvp.Value.Update(currentTime);
            }
        }
       
        public void RegisterArea(IArea a)
        {
            AllAreas.AddOrUpdate(a.Id, a, (k, v) => a);

            if (a is PSystem)
            {
                idToSystem.AddOrUpdate(a.Id, (PSystem)a, (k, v) => (PSystem)a);
                _localSystems.AddOrUpdate(a.Id, (PSystem)a, (k, v) => (PSystem)a);
            }

            if (a is Port)
                AllPorts.AddOrUpdate(a.Id, (Port)a, (k, v) => (Port)a);

        }
        
        public void RegisterSimulatable(ISimulatable s)
        {
            AllSimulatableObjects.AddOrUpdate(s.Id, (ISimulatable)s, (k, v) => (ISimulatable)s);

        }
       
        /// <summary>
        /// Returns the ID, deregesters the ID from all dictionaries. May benefit from optimization
        /// </summary>
        /// <param name="ID"></param>
        public void DeRegisterArea(int ID)
        {
            PSystem tempSys;
            Port tempPort;
            IArea a;

            //Maybe a type check is faster than TryRemove?
            
            idToSystem.TryRemove(ID, out tempSys); // Can be changed to reference to System
            AllPorts.TryRemove(ID, out tempPort);
            _localSystems.TryRemove(ID, out tempSys);
            AllAreas.TryRemove(ID, out a);

        }        

        /// <summary>
        /// Gets an area being handled by this server, null if the area is non-local
        /// NOTE: If ID==null, GetArea returns limbo
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public IArea GetArea(int? ID)
        {
            if (ID == null)
                return null;

            IArea outArea;
            AllAreas.TryGetValue((int)ID, out outArea);

            if (outArea == null)
            {
                ConsoleManager.WriteLine("Non-local area requested from GetLocalArea", ConsoleMessageType.Warning);
            }
            return outArea;
        }
            

        /// <summary>
        /// Checks if a systemID references a locally handled system
        /// </summary>
        /// <param name="systemID"></param>
        /// <returns></returns>
        public bool IsLocalSystem(int? systemID)
        {   
            return systemID == null || _localSystems.ContainsKey((int)systemID);
        }

        /// <summary>
        /// Checks if an areaId references a locally handled area
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public bool IsLocalArea(int? areaId)
        {
            return areaId == null || AllAreas.ContainsKey((int)areaId);
        }

        /// <summary>
        /// This will probably be removed, just for testing db 
        /// </summary>
        /// <returns></returns>
        public List<IArea> GetAllAreas()
        {
            var al = new List<IArea>();
            foreach (var v in AllAreas)
                al.Add(v.Value);

            return al;

        }

        /// <summary>
        /// Probably should implement recursive deregistration for ships and areas eventually
        /// </summary>
        public void ClearLocalSystems()
        {
            foreach(var p in _localSystems)
            {
                DeRegisterArea(p.Value.Id);
            }
            _localSystems.Clear();
        }

        /// <summary>
        /// Returns true if colonization succesful
        /// </summary>
        /// <param name="colonizingShip"></param>
        /// <param name="xPos">ref in case positions are modified to accomodate placement (e.g. snap to nearest tile)</param>
        /// <param name="yPos">ref in case positions are modified to accomodate placement (e.g. snap to nearest tile)</param>
        /// <returns></returns>
        public bool TryColonizePlanet(Planet planet, IShip colonizingShip, WarpManager wm, LocatorService ls, float xPos, float yPos, out string resultMessage, IDatabaseManager databaseManager)
        {
            if (planet.IsColonized)
            {
                resultMessage = "Planet is already colonized!";
                return false;
            }
            else if (colonizingShip.CurrentEnergy != colonizingShip.ShipStats.Energy)//Is energy full?
            {
                resultMessage = "Not enough energy.";
                return false;
            }
            else if (!planet.GetValidStructurePosition(new CommandCenterStats(), ref xPos, ref yPos))//Check for wall overlap here
            {
                resultMessage = "Invalid placement location.";
                return false;
            }
            else if (colonizingShip.Cargo.GetCargoAmount(StatelessCargoTypes.Biodome) > 0)
            {//if everything checks out

                ColonyFactory.CreateColony(xPos, yPos, colonizingShip.GetPlayer(), planet, ls);

                List<IShip> ShipsToMove = new List<IShip>(1);
                //Kick all non-allied players into space
                foreach (var s in planet.GetShips())
                {
                    if (s.Value != colonizingShip)
                        if (s.Value is NPCShip && !_teamLocator.AreAllied(((NPCShip)s.Value), planet.GetOwner()))
                            ShipsToMove.Add(s.Value);
                        else if (s.Value is PlayerShip && !_teamLocator.AreAllied(s.Value.GetPlayer(), planet.GetOwner()))
                            ShipsToMove.Add(s.Value);
                }

                foreach (var s in ShipsToMove)
                {
                    wm.ChangeArea((int)planet.ParentAreaID, s, false, true);
                }
                resultMessage = "Colonization Successful.";
                return true;
            }
            else
            {
                resultMessage = "No biodome found in ship cargo.";
                return false;
            }
        }

       
    }
    
}