using System;
using System.Collections.Generic;
using System.Linq;
using Freecon.Core.Interfaces;
using Server.Managers;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models;
using SRServer.Services;
using Server.Models.Structures;
using Server.Models.Extensions;
using Server.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Utils;
using Server.Models.Mathematics;

namespace Server.Models
{
    public class PSystem : Area<PSystemModel>
    {
        #region UI Variables
        
        /// <summary>
        /// Only used for Server UI
        /// </summary>
        public bool HasPort
        {
            get
            {
                if (_model.PortIDs.Count() > 0) return true;
                else return false;
            }
        }

        /// <summary>
        /// Only used for Server UI
        /// </summary>
        public int WarpholeCount
        {
            get { return Warpholes.Count(); }
        }

        /// <summary>
        /// Only used for Server UI
        /// </summary>
        public string PlayersInSystem
        {
            get
            {
                string s = "";
                if (_model.ShipIDs.Count > 0)
                {
                    foreach (var ss in GetShips())
                    {
                        s += ss.Value.GetPlayer().Username;
                    }
                }
                else s = "Empty System";
                return s;
            }
        }

        #endregion

        //File format
        //SystemName    ID    XPos    YPos   starID   numLinkedTo     linkIDs     numPlanets     planetIDs    
        //SystemName must be given without spaces, underscores are processed and removed

        //public List<IShip> ships;
        //public List<Warphole> Warpholes;

        public int NumberOfPlanets;
        public bool HasTwoOrMore;
        public bool IsCluster;

        public IReadOnlyList<int> MoonIDs { get { return (IReadOnlyList<int>)_model.MoonIDs.ToList(); } }

        public int MoonCount { get { return _model.MoonIDs.Count; } }

        public IReadOnlyList<int> PlanetIDs { get { return (IReadOnlyList<int>)_model.PlanetIDs.ToList(); } }

        public int PlanetCount { get { return _model.PlanetIDs.Count; } }

        public IReadOnlyList<int> PortIDs { get { return (IReadOnlyList<int>)_model.PortIDs.ToList(); } }

        public Star Star{get { return _model.Star; }set { _model.Star = value; } } //WARNING: Needs to be initialized properly.
        public int? StarBaseID { get { return _model.StarBaseID; } set { _model.StarBaseID = value; } }

        // System Statistics

        protected PSystem()
        {
        }

        public PSystem(PSystemModel s, LocatorService ls)
            : base(s, ls)
        {
            

            

        }

        public PSystem(Star star, int ID, LocatorService ls)
            : base(ID, ls)
        {
            Star = star;             
        }

        public PSystem(int xPos, int yPos, int ID, LocatorService ls)
            : base(ID, ls)
        {
            PosX = xPos;
            PosY = yPos;
        }

        public override void SendEntryData(HumanPlayer sendHere, bool warping, IShip playerShip)
        { 
            //Legacy bug, probably fixed.
            if(_model.ShipIDs.Count == 0)
            {
                ConsoleManager.WriteLine("ERROR: IShip wasn't in PSystem.ships on warp. Probably a db error. Adding IShip to system.", ConsoleMessageType.Error);
                _model.ShipIDs.Add(playerShip.Id);
                _shipCache.Add(playerShip.Id, playerShip);
            }
            
            MessageTypes messageType;

            messageType = warping ? MessageTypes.WarpApproval : MessageTypes.StarSystemData;

            var s = GetEntryData(playerShip.Id, false, true);
            s.NewPlayerXPos = playerShip.PosX;
            s.NewPlayerYPos = playerShip.PosY;

            sendHere.SendMessage(s, messageType);
            
        }

        /// <summary>
        /// if playerShipID is not null, ship data matching playerShipID won't be sent.
        /// </summary>
        /// <param name="playerShipID"></param>
        /// <param name="sendCargo">Always true for simulator, hould probably be false for clients</param>
        /// <returns></returns>
        public override AreaEntryData GetEntryData(int? playerShipID, bool sendCargo = false, bool writeShipStats = false)
        {
            var starData = new StarData()
            {
                Radius = Star.Radius,
                Density = Star.Density,
                InnerGravityStrength = Star.InnerGravityStrength,
                OuterGravityStrength = Star.OuterGravityStrength,
                Type = Star.Type
            };

            var entryData = new SystemEntryData(base.GetEntryData(playerShipID, sendCargo, writeShipStats))
            {
                Id = Id,
                AreaName = AreaName,
                AreaSize = AreaSize,
                StarData = starData
            };
            
            List<Planet> planets = GetPlanets();
            foreach (Planet p in planets)
            {
                entryData.Planets.Add(new PlanetData_SystemView() { Mass = (byte)p.Mass, Gravity = (byte)(p.Gravity * 100f), Distance = (Int16)p.Distance, MaxTrip = p.MaxTrip, CurrentTrip = p.CurrentTrip, Scale = (byte)p.Scale, PlanetType = p.PlanetType, IDToOrbit = p.IDToOrbit, Id = p.Id });

            }

            List<Planet> moons = GetMoons();
            foreach (Planet p in moons)
            {
                entryData.Moons.Add(new MoonData_SystemView() { Scale = p.Scale, Distance = (Int16)p.Distance, MaxTrip = p.MaxTrip, CurrentTrip = p.CurrentTrip, PlanetType = p.PlanetType, IDToOrbit = p.IDToOrbit, Id = p.Id });
            }
            
            foreach (int p in _model.PortIDs)
            {
                Port pp = (Port)_areaLocator.GetArea(p);
                entryData.Ports.Add(new PortData_SystemView() { CurrentTrip = pp.CurrentTrip, Distance = (Int16)pp.Distance, Id = pp.Id, IDToOrbit = pp.IDToOrbit, IsMoon = pp.IsMoon, MaxTrip = pp.MaxTrip });
            }
            
            entryData.SecurityLevel = SecurityLevel;
          
            return entryData;
        }

        public override void SetEntryPosition(IHasPosition warpingObject, IArea oldArea)
        {
            if (oldArea == null)
            {
                warpingObject.PosX = 0;
                warpingObject.PosY = 0;
                warpingObject.SetRandomPointInRadius(Star.Radius + .1f, Star.Radius + 1f);
            }
            else
            {
                switch (oldArea.AreaType)
                {
                    case AreaTypes.System:
                        {
                            SetEntryPositionFromSystem(warpingObject);
                            break;
                        }
                    case AreaTypes.Planet:
                    case AreaTypes.Port:
                    case AreaTypes.StarBase:
                        {
                            warpingObject.PosX = oldArea.PosX;
                            warpingObject.PosY = oldArea.PosY;
                            warpingObject.SetRandomPointInRadius(oldArea.AreaSize / 100f + .1f, oldArea.AreaSize / 100f + 1.5f);
                            break;
                        }

                    case AreaTypes.Colony:
                        {
                            var pa = oldArea.GetParentArea();
                            float parentPosX = pa.PosX;
                            float parentPosY = pa.PosY;
                            SpatialOperations.GetRandomPointInRadius(ref parentPosX, ref parentPosY, pa.AreaSize / 100f + .1f, pa.AreaSize / 100f + 1.5f);
                            warpingObject.PosX = parentPosX;
                            warpingObject.PosY = parentPosY;
                            break;
                        }
                }
            }
            
        }

        /// <summary>
        /// Special overload for handoffs
        /// </summary>
        /// <param name="warpingObject"></param>
        /// <param name="oldSystemModel"></param>
        public void SetEntryPositionFromSystem(IHasPosition warpingObject)
        {
            //Flip to opposite side to match entry warphole position
            warpingObject.PosX *= -1;
            warpingObject.PosY *= -1;
            warpingObject.SetRandomPointInRadius(.75f, 1.5f);
        }

        /// <summary>
        /// Adds structure with passed parameters
        /// returns structure ID
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public override void AddStructure(IStructure s)
        {
            base.AddStructure(s);

            if (_onlinePlayerIDs.Count > 0)
            {
                var data = new MessageReceiveNewStructure();
                data.StructureData = s.GetNetworkData();
                BroadcastMessage(new NetworkMessageContainer(data, MessageTypes.ReceiveNewStructure));
            }         
               

        }

        /// <summary>
        /// Checks if the structure can be added according to various rules
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buildingType"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public override bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage)
        {

            if ((player.CurrentAreaID != Id))
            {
                resultMessage = "Player not in system.";
                return false;
            }



            resultMessage = "Success";
            return true;
            


        }

        public List<Planet> GetPlanets()
        {
            List<Planet> l = new List<Planet>();
            foreach (int i in _model.PlanetIDs)
                l.Add((Planet)_areaLocator.GetArea(i));

            return l;
        }

        public void RemovePlanet(Planet p)
        {
            _model.PlanetIDs.Remove(p.Id);
        }

        public void AddPlanet(Planet p)
        {
            _model.PlanetIDs.Add(p.Id);
        }

        public List<Planet> GetMoons()
        {
            List<Planet> l = new List<Planet>();
            foreach (int i in _model.MoonIDs)
                l.Add((Planet)_areaLocator.GetArea(i));

            return l;
        }

        public void AddMoon(Planet m)
        {
            _model.MoonIDs.Add(m.Id);
        }

        public void RemoveMoon(Planet m)
        {
            _model.MoonIDs.Remove(m.Id);
        }

        public void AddPort(Port p)
        {
            _model.PortIDs.Add(p.Id);
        }
    
        public void RemovePort(Port p)
        {
            _model.PortIDs.Remove(p.Id);
        }

        public override bool GetValidStructurePosition(StructureStats stats, ref float xPos, ref float yPos)
        {
            bool isValid = true;

            //Check for overlap with structures
            for (int i = 0; i < _structures.Count; i++ )
            {
                var s = _structures.ElementAt(i);
                isValid = !s.Value.CheckOverlap(stats.StructureSizeX, stats.StructureSizeY, xPos, yPos);

#if DEBUG
                if (!isValid)
                    ConsoleManager.WriteLine("Overlapping with an existing structure; setting isValid to false.");
#endif
                if (!isValid)
                    break;

            }

           

            foreach(Planet p in GetPlanets())
            {
                isValid = !p.CheckOverlap(stats.StructureSizeX, stats.StructureSizeY, xPos, yPos);
                if (!isValid)
                    break;
            }

            return isValid;
        
        }
        
        public IEnumerable<Port> GetPorts()
        {
            List<Port> l = new List<Port>(_model.PortIDs.Count);
            foreach (var i in _model.PortIDs)
                l.Add((Port)_areaLocator.GetArea(i));

            return l;
        }

    }

    
}