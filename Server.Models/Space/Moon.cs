//using System;
//using Freecon.Models.TypeEnums;
//using SRServer.Services;
//using Server.Interfaces;
//using Freecon.Core.Networking.Models;
//using Server.Models.Structures;
//using System.Collections.Generic;
//using Server.Managers;
//using Freecon.Core.Interfaces;
//using Freecon.Core.Networking.Models.Objects;
//using Freecon.Core.Networking.Models.Messages;
//using Freecon.Core.Utils;

//namespace Server.Models
//{

//    public class Moon : Area<MoonModel>
//    {//TODO: Set up inheritance between this and Planet?


//        public PlanetLayout Layout { get { return _layout; } set { _model.LayoutId = value.Id; _layout = value; } }
//        private PlanetLayout _layout;

//        public const float TileSize = 1f;

//        public byte Scale { get { return _model.Scale; } set { _model.Scale = value; } }
//        public PlanetTypes PlanetType { get { return _model.PlanetType; } set { _model.PlanetType = value; } }

//        protected Moon()
//        {
//        }

//        public Moon(MoonModel m, PlanetLayout layout, LocatorService ls)
//            : base(m, ls)
//        {

//            //ColonyID = p.Colony == null? null : (int?)p.Colony.Id;
//            Scale = m.Scale;
//            Layout = layout;
//            PlanetType = m.PlanetType;
//            _shipCache = new Dictionary<int, IShip>();


//        }

//        public Moon(int numberOrbit, int minDistance, int incrementOfOrbit, int variationOfOrbit, Planet parent,
//                    int parentID, int ID, int randomSeed, LocatorService ls)
//            : base(ID, ls)
//        {
//            CreateMoon(numberOrbit, minDistance, incrementOfOrbit, variationOfOrbit, parent, parentID, ID, randomSeed);
//        }

//        /// <summary>
//        /// Creates a moon with a wide range of options.
//        /// </summary>
//        /// <param name="numberOrbit">Which orbit outwards (I, IV)</param>
//        /// <param name="minDistance">Minimum distance, added to value of increment</param>
//        /// <param name="incrementOfOrbit">How far apart the base distance between moons</param>
//        /// <param name="variationOfOrbit">Fluctuation in distance between moons</param>
//        /// <param name="parent">Planet to orbit</param>
//        /// <param name="ID">ID to use when landing</param>
//        /// <param name="randomSeed">Seed used for good randoms</param>
//        public void CreateMoon(int numberOrbit, int minDistance, int incrementOfOrbit, int variationOfOrbit,
//                               Planet parent, int parentID, int ID, int randomSeed)
//        {
//            var r = new Random(randomSeed);


//            Distance = (minDistance + ((incrementOfOrbit + r.Next(0, variationOfOrbit)) * numberOrbit));
//            // Generate a moon distance from the star.

//            MaxTrip = Distance * Distance * 1000;
//            CurrentTrip = r.Next(0, MaxTrip); // Set random angle

//            Scale = (byte)r.Next(1, 4); // Generate a scale

//            PlanetType = (PlanetTypes)r.Next(0, (int)PlanetTypes.OceanicSmall + 1); // Generate a planet type 

//            IDToOrbit = parentID;

//        }

//        /// <summary>
//        /// Warping is true for landing on a planet, false for leaving a building
//        /// </summary>
//        /// <param name="sendHere"></param>
//        /// <param name="warping"></param>
//        /// <param name="playerShip"></param>
//        public override void SendEntryData(HumanPlayer sendHere, bool warping, IShip playerShip)
//        {


//#if DEBUG
//            //Legacy bug, probably fixed
//            if (_model.ShipIDs.Count == 0)
//            {
//                ConsoleManager.WriteLine("ERROR: IShip wasn't in Planet.ships on warp. Probably a db error. Adding IShip to system.", ConsoleMessageType.Error);
//                _model.ShipIDs.Add(playerShip.Id);
//                _shipCache.Add(playerShip.Id, playerShip);
//            }

//#endif

//            var data = GetEntryData(playerShip.Id);

//            data.NewPlayerXPos = playerShip.PosX;
//            data.NewPlayerYPos = playerShip.PosY;

//            sendHere.SendMessage(new NetworkMessageContainer(data, MessageTypes.MoonLandApproval));
//        }

//        /// <summary>
//        /// if playerShipID is not null, ship data matching playerShipID won't be sent.
//        /// </summary>
//        /// <param name="playerShipID"></param>
//        /// <param name="sendCargo">Always true for simulator, hould probably be false for clients</param>
//        /// <returns></returns>
//        public override AreaEntryData GetEntryData(int? playerShipID, bool sendCargo = false, bool writeShipStats = false)
//        {
//            MoonEntryData data = new MoonEntryData(base.GetEntryData(playerShipID, sendCargo));


//            data.Layout = Layout.GetNetworkData();



//            return data;

//        }

//        public void AddWarpholeAtLocation(float x, float y)
//        {
//            this.Warpholes.Add(new Warphole(x, y, (int)Id, (int)ParentAreaID, (byte)Warpholes.Count));
//        }

//        public override void RemoveShip(IShip s)
//        {
//            #region changing player position to match new Warphole position


//            s.PosX = (PosX - Rand.Random.Next(-10, 10) / 10f); //Randomize position
//            s.PosY = (PosY - Rand.Random.Next(-10, 10) / 10f); //Randomize position
//            ConsoleManager.WriteLine(s.PosX + "," + s.PosY);
//            #endregion

//            base.RemoveShip(s);

//        }

//        /// <summary>
//        /// Adds structure with passed parameters
//        /// returns structure ID
//        /// </summary>
//        /// <param name="player"></param>
//        /// <param name="buildingType"></param>
//        /// <param name="xPos"></param>
//        /// <param name="yPos"></param>
//        /// <returns></returns>
//        public override void AddStructure(IStructure s)
//        {
//            base.AddStructure(s);

//            if (_onlinePlayerIDs.Count > 0)
//            {
//                var data = new MessageReceiveNewStructure();
//                data.StructureData = s.GetNetworkData();
//                BroadcastMessage(new NetworkMessageContainer(data, MessageTypes.ReceiveNewStructure, Id));
//            }
//        }

//        /// <summary>
//        /// Checks if the structure can be added according to various rules
//        /// </summary>
//        /// <param name="player"></param>
//        /// <param name="buildingType"></param>
//        /// <param name="xPos"></param>
//        /// <param name="yPos"></param>
//        /// <returns></returns>
//        public override bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage)
//        {
//            resultMessage = "Cannot place structures on moons.";
//            return false;
//        }



//        /// <summary>
//        /// Removes a IShip from the previous area, adds IShip to this planet, changes IShip location to random spot near warphole
//        /// </summary>
//        /// <param name="s"></param>
//        public override void MoveShipHere(IShip s)
//        {
//            base.MoveShipHere(s);

//            #region changing player position to match new Warphole position

//            s.PosX = (Warpholes[0].PosX + Rand.Random.Next(-2, 2)); //Randomize position
//            s.PosY = (Warpholes[0].PosY + Rand.Random.Next(-2, 2)); //Randomize position

//            #endregion
//        }



//        public override IDBObject GetDBObject()
//        {
//            return _model.GetClone();
//        }
//    }
//}