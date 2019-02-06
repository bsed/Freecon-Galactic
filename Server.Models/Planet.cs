using System;
using System.Collections.Generic;
using Lidgren.Network;
using Server.Managers;
using System.IO;
using System.Linq;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models;
using SRServer.Services;
using Server.Interfaces;
using Server.Models.Structures;
using MongoDB.Bson.Serialization.Attributes;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Utils;
using Server.Models.Mathematics;


namespace Server.Models
{
    public class Planet : Area<PlanetModel>
    {
        public const float TileSize = 1f;

        // Consider make a sub component of this that holds data for the types. OutsidePlanet; InsidePlanet
        // Might make this some sort of data holder... Perhaps alongside? For randomly generated planets.

        public float Gravity { get { return _model.Gravity; } set { _model.Gravity = value; } }

        public bool HasMoons { get { return _model.HasMoons; } set { _model.HasMoons = value; } }
        
        public bool IsColonized {get { return _model.IsColonized; } set { _model.IsColonized = value; } }

        public int? ColonyID { get { return _model.ColonyID; } protected set { _model.ColonyID = value; } }

        public int? OwnerDefaultTeamId { get { return _model.OwnerDefaultTeamId; } protected set { _model.OwnerDefaultTeamId = value; } }//Decoupled from Owner to prevent excessive database reads when the player is handled on another server

        public PlanetLayout Layout { get { return _layout; } set { _model.LayoutId = value.Id; _layout = value; } }
        private PlanetLayout _layout;

        public int Mass{ get { return _model.Mass; } set { _model.Mass = value; } }
        public PlanetTypes PlanetType{ get { return _model.PlanetType; } set { _model.PlanetType = value; } }
        public byte Scale{ get { return _model.Scale; } set { _model.Scale = value; } }
        public bool IsMoon { get { return _model.IsMoon; } set { _model.IsMoon = value; } }
        ITeamLocator _teamLocator;

        public Planet(PlanetModel p, PlanetLayout layout, LocatorService ls)
            : base(p, ls)
        {
            Gravity = p.Gravity;
            HasMoons = p.HasMoons;
            IsColonized = p.IsColonized;
            //ColonyID = p.Colony == null? null : (int?)p.Colony.Id;
            ColonyID = p.ColonyID;
            Layout = layout;
            Mass = p.Mass;
            PlanetType = p.PlanetType;
            Scale = p.Scale;

            _shipCache = new Dictionary<int, IShip>();

            _teamLocator = ls.TeamLocator;

        }

        public Planet(IArea parentArea, int ID, LocatorService ls)
            : base(ID, ls)
        {
            ParentAreaID = parentArea.Id;
            _teamLocator = ls.TeamLocator;
            _areaLocator = ls.AreaLocator;

            _shipCache = new Dictionary<int, IShip>();
            _model.ShipIDs = new HashSet<int>();
            _model.StructureIDs = new HashSet<int>();
            Scale = 10;
            SecurityLevel = 0;
            Warpholes = new List<Warphole>(1);



        }                
  
        /// <summary>
        /// Creates a planet with a wide range of options
        /// </summary>
        /// <param name="numberOrbit">Which orbit outwards (I, IV)</param>
        /// <param name="moonChance">Moon spawn chance, in %</param>
        /// <param name="minDistance">Minimum distance, added to value of increment</param>
        /// <param name="incrementOfOrbit">How far apart the base distance between moons</param>
        /// <param name="variationOfOrbit">Fluctuation in distance between moons</param>
        /// <param name="starID">Star to orbit</param>
        /// <param name="ID">ID to use when landing</param>
        /// <param name="randomSeed">Seed used for good randoms</param>
        public void Init(int numberOrbit, int moonChance, int minDistance, int incrementOfOrbit,
                                 int variationOfOrbit, int starID, int randomSeed)
        {
            var r = new Random(randomSeed);
            float angle;

            Mass = r.Next(5, 30);
            Gravity = 0.05f*r.Next(50, 100);

            // If 0, no moons. If 1, has moons.
            int getRandomBool = r.Next(0, 100);
            bool hasMoons;
            if (getRandomBool > (100 - moonChance)) // Gets a percent. 100% chance is 0, 100-100
                hasMoons = false;
            else
            {
                hasMoons = true;
            }
            
            Distance = (minDistance + ((incrementOfOrbit + r.Next(0, variationOfOrbit))*numberOrbit));
            
            MaxTrip = Distance*Distance;
            CurrentTrip = r.Next(0, MaxTrip); // Set random angle

            Scale = (byte)r.Next(4, 11); // Generate a scale (Random works with +10 for max value. C# quirk)
            if (Scale < 6)
                hasMoons = false;

            PlanetType = (PlanetTypes)r.Next(0, (int) PlanetTypes.Frozen + 1); // Generate a planet type 

            IDToOrbit = starID; // Always the sun, but you know. Could be binary!

            angle = 2f * (float)Math.PI * ((float)CurrentTrip/(float)MaxTrip);

            PosX = (float)Math.Cos(angle) * Distance / 100f;
            PosY = (float)Math.Sin(angle) * Distance / 100f;

            this.HasMoons = hasMoons;
        }

        /// <summary>
        /// Warping is true for landing on a planet, false for leaving a building
        /// </summary>
        /// <param name="sendHere"></param>
        /// <param name="warping"></param>
        /// <param name="playerShip"></param>
        public override void SendEntryData(HumanPlayer sendHere, bool warping, IShip playerShip)
        {
#if DEBUG
            //Legacy bug, probably fixed
            if (_model.ShipIDs.Count == 0)
            {
                ConsoleManager.WriteLine("ERROR: IShip wasn't in Planet.ships on warp. Probably a db error. Adding IShip to system.", ConsoleMessageType.Error);
                _model.ShipIDs.Add(playerShip.Id);
                _shipCache.Add(playerShip.Id, playerShip);
            }
#endif
            var data = GetEntryData(playerShip.Id, false, true);
            data.NewPlayerXPos = playerShip.PosX;
            data.NewPlayerYPos = playerShip.PosY;


            sendHere.SendMessage(new NetworkMessageContainer(data, MessageTypes.PlanetLandApproval));
        }

        /// <summary>
        /// if playerShipID is not null, ship data matching playerShipID won't be sent.
        /// </summary>
        /// <param name="playerShipID"></param>
        /// <param name="sendCargo">Always true for simulator, hould probably be false for clients</param>
        /// <returns></returns>
        public override AreaEntryData GetEntryData(int? playerShipID, bool sendCargo = false, bool writeShipStats = false)
        {
            PlanetEntryData data = new PlanetEntryData(base.GetEntryData(playerShipID, sendCargo, writeShipStats));
            data.Id = Id;

            // If the planet is colonized, send the owner's team ID
            if (IsColonized)
            {
                data.PlanetTeamID = OwnerDefaultTeamId;
            }
            
            data.PlanetType = PlanetType;
            data.Layout = Layout.GetNetworkData();
            
            return data;
        }

        public void AddWarpholeAtLocation(float x, float y)
        {
            Warpholes.Add(new Warphole(x, y, (int)Id, (int)ParentAreaID, (byte)Warpholes.Count));
        }
               
        public override void RemoveShip(IShip s)
        {
            #region changing player position to match new Warphole position
                     
            
            s.PosX = (PosX - Rand.Random.Next(-10, 10) / 10f); //Randomize position
            s.PosY = (PosY - Rand.Random.Next(-10, 10) / 10f); //Randomize position
            ConsoleManager.WriteLine(s.PosX + "," + s.PosY);
            #endregion
            
            base.RemoveShip(s);

        }

        /// <summary>
        /// Adds structure with passed parameters
        /// returns structure ID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="buildingType"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        public override void AddStructure(IStructure s)
        {
            base.AddStructure(s);

            if (IsColonized)
                GetColony().RegisterStructure(s);
            
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
            if(player.CurrentAreaID != Id )
            {
                resultMessage = "Player is not on the planet.";
                return false;
            }
            else if (!player.ColonizedPlanetIDs.Contains(Id) || (IsColonized && !player.GetTeamIDs().Overlaps(GetOwner().GetTeamIDs())))
            {
                resultMessage = "Cannot place structures on unallied planets.";
                return false;
            }
            else
            {
                resultMessage = "Success";
                return true;
            }

        }

        public override bool GetValidStructurePosition(StructureStats stats, ref float xPos, ref float yPos)
        {
            bool isValid = base.GetValidStructurePosition(stats, ref xPos, ref yPos);
            if (!isValid)
                return false;



            //Check for overlap with wall tiles
            int numX = (int)Math.Ceiling(stats.StructureSizeX / TileSize);
            int numY = (int)Math.Ceiling(stats.StructureSizeY / TileSize);

            float startX = xPos - (numX/2) * TileSize;
            float startY = yPos - (numY/2) * TileSize;

            for(int i = 0; i < numX; i++)
            {
                float xCoord = startX + i * TileSize;
                for (int j = 0; j < numY; j++)
                {
                    float yCoord = startY + j * TileSize;
                   
#if DEBUG
                    bool val = !Layout.IsWallTile(xCoord, yCoord);
                    //if (!val)
                        //ConsoleManager.WriteToFreeLine("Structure overlapping with wall.", ConsoleMessageType.Debug);
                    isValid = val;

                    if (!isValid)
                        break;

                    val = isValid = Layout.IsInBounds(xCoord, yCoord);
                    //if (!val)
                        //ConsoleManager.WriteToFreeLine("Structure out of bounds.", ConsoleMessageType.Debug);
                    if (!isValid)
                        break;

#else
                    isValid = Layout.IsInBounds(xCoord, yCoord) && !Layout.IsWallTile(xCoord, yCoord);
                    if(!isValid)
                        break;
#endif
                }
                if (!isValid)
                    break;
            }
                        

            return isValid;
        }

        public override void SetEntryPosition(IHasPosition warpingObject, IArea oldArea)
        {
            //TODO: Replace with starport style safe zone
            float xpos = Warpholes[0].PosX;
            float ypos = Warpholes[0].PosY;
            SpatialOperations.GetRandomPointInRadius(ref xpos, ref ypos, .5f, 3);
            warpingObject.PosX = xpos;
            warpingObject.PosY = ypos;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>null if !IsColonized, or if the player is being handled on another server</returns>
        public Player GetOwner()
        {
            //TODO: Fix this to work with players handled on other servers. May require some refactoring/rethinking
            if (!IsColonized)
                return null;

            var c = GetColony().OwnerID;
            var p = _playerLocator.GetPlayerAsync(c).Result;
            return p;
        }

        public Planet SetColony(Colony c)
        {
            ColonyID = c.Id;
            IsColonized = true;
            OwnerDefaultTeamId = c.OwnerDefaultTeamID;
            return this;
        }

        public Colony GetColony()
        {
            Colony c = _areaLocator.GetArea(ColonyID) as Colony;
            return c;
        }

        public void RemoveColony()
        {
            ColonyID = null;
            IsColonized = false;
        }

        public override IDBObject GetDBObject()
        {
            return _model.GetClone();
        }

        /// <summary>
        /// Sends capture message to all client on the planet. Must call after all capture logic is complete. Remember to boot all non allied players from the planet first!
        /// </summary>
        public void SendCaptureMessage()
        {
            var data = new MessageColonyCaptured();
            data.OwnerTeamID = GetOwner().DefaultTeamID;
            BroadcastMessage(new NetworkMessageContainer(data, MessageTypes.ColonyCaptured));
            
        }

    }

    /// <summary>
    /// Used when you land on a planet. Represents the invadable area.
    /// </summary>
    public class PlanetLayout : ISerializable, IDBObject
    {         
        public bool[] Layout1D { get; set; }

        public int Id { get; set; }
         
        public List<Warphole> Warpholes { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.PlanetLayout; } }

        public PlanetLayout() 
        {
            Warpholes = new List<Warphole>();
        }
         
        public IEnumerable<float> PositionsX
        {
            get
            {
                return Warpholes.Select(p => p.PosX);
            }
        }

         
        public IEnumerable<float> PositionsY
        {
            get
            {
                return Warpholes.Select(p => p.PosY);
            }
        }

        public int NumX {get; set;}
            
        public int NumY {get;set;} // Scale of the 1d array
         
         
        public string LayoutFile { get; set; }

        [BsonId]         
        public string LayoutName { get; set; }


        public PlanetLayout(string filename)
        {

            LayoutFile = filename;
            LayoutName = filename.Split(new char[] { '\\', '/' }).Last();

            Warpholes = new List<Warphole>(1);

            // Opens file and reads
            string[] strings = File.ReadAllLines(filename);

            // Gets dimensions of the array to read
            string[] dimensions = strings[0].Split(' ');
            NumX = int.Parse(dimensions[0]);
            NumY = int.Parse(dimensions[1]);

            // One Dimensional array to hold data
            Layout1D = new bool[NumX * NumY];

            int count = -1; // Easy way to know where we are in the array
            foreach (string s in strings) // Each line in array
            {
                if (count == -1) // First line holds the dimensions, lets skip it
                {
                    count++;
                    continue;
                }
                // Each character is a tile
                for (int i = 0; i < s.Length; i++)
                {
                    int val = int.Parse(s[i].ToString());
                    switch(val)
                    {
                        case 0://Wall
                            Layout1D[count] = false;
                            Tuple<float, float> tileCoords = _indexToCoords(count);
                            
                            break;

                        case 1://Not wall
                            Layout1D[count] = true;
                            break;

                        case 2:
                            Layout1D[count] = true;
                            Tuple<float, float> warpCoords = _indexToCoords(count);

                            Warpholes.Add(new Warphole(warpCoords.Item1, warpCoords.Item2));
                                                        
                            break;
                    }

                   
                    count++;
                }
            }
        }
           
        public bool IsWallTile(float xLoc, float yLoc)
        {
            int index = _coordsToIndex(xLoc, yLoc);
            return !Layout1D[index];

        }

        public bool IsInBounds(float xLoc, float yLoc)
        {
            return (xLoc < NumX-1 && yLoc < NumY-1 && xLoc > 0 && yLoc > 0);
        }

        /// <summary>
        /// Returns the index to the tile nearest to the passed coordinates
        /// </summary>
        /// <param name="xCoords"></param>
        /// <param name="yCoords"></param>
        /// <returns></returns>
        public int _coordsToIndex(float xCoords, float yCoords)
        {

            int xIndex = (int)Math.Floor(xCoords / Planet.TileSize);
            int yIndex = (int)Math.Floor(yCoords / Planet.TileSize);
            int index = yIndex * NumX + xIndex;

            
            if (xIndex > NumX || xIndex < 0)
            {
                ConsoleManager.WriteLine("Warning: coordinates are out of planet layout bounds. Returning last tile.", ConsoleMessageType.Warning);
                return Layout1D.Count() - 1;
            }
            else if (yIndex > NumY || yIndex < 0)
            {
                ConsoleManager.WriteLine("Warning: coordinates are out of planet layout bounds. Returning last tile.", ConsoleMessageType.Warning);
                return Layout1D.Count() - 1;
            }
            else if (index >= Layout1D.Count())
            {
                ConsoleManager.WriteLine("Warning: coordinates are out of planet layout bounds. Returning last tile.", ConsoleMessageType.Warning);
                return Layout1D.Count() - 1;
            }
            else if (index < 0)
            {
                ConsoleManager.WriteLine("Warning: coordinates are out of planet layout bounds. Returning first tile.", ConsoleMessageType.Warning);
                return 0;
            }
            else
            {
                return index;
            }

        }
    

        /// <summary>
        /// Returns the X and Y coordinates, based on the index in the 1D array
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Tuple<float, float> _indexToCoords(int index)
        {
            // Convert count from 1D into 2D
            int locX, locY;
            locY = (int)Math.Floor(index / (float)NumY);
            locX = index % NumX; // We can't mod by 0


            return new Tuple<float, float>(locX * Planet.TileSize, locY * Planet.TileSize);



        }

        public LayoutData GetNetworkData()
        {
            LayoutData data = new LayoutData();
            data.NumX = NumX;
            data.NumY = NumY;

            data.Layout1D = Layout1D;

            return data;
        }

        public PlanetLayout Deserialize(NetOutgoingMessage message)
        {
            throw new NotImplementedException();
        }
        
        public class DatabaseVector2
        {
            public int Id { get; set; }//Fucking entity framework

            public float X { get; set; }

            public float Y { get; set; }
        }

        public class Island
        {
            public int Id { get; set; }
            public ICollection<DatabaseVector2> Points { get; set; }
        }


        public IDBObject GetDBObject()
        {
            return this;
        }

    }

   
}
