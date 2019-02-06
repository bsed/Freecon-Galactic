using Core.Models.CargoHandlers;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Objects;
using Freecon.Models;
using MongoDB.Bson.Serialization.Attributes;
using Server.Interfaces;
using System;


namespace Server.Models.Economy
{
    
    //Records a trade between two entities
    public class Trade:TradeRecordModel
    {
        [BsonIgnore]
        public float LastUpdateTime;


        /// <summary>
        /// Trade is not yet being processed
        /// </summary>
        [BsonIgnore]
        public bool Pending { get; set; }

        [BsonIgnore]
        public bool IsProcessed { get; set; }

        
        //Entity A
        [UICollection]
        public CargoHandlerModel CargoOffered_A { get; set; }
        
        [BsonIgnore]
        [UIProperty]
        public bool Accepted_A { get; set; }

        //Entity B
        [UICollection]
        public CargoHandlerModel CargoOffered_B { get; set; }
        
        [BsonIgnore]
        [UIProperty]
        public bool Accepted_B { get; set; }

        protected Trade() { }

        public Trade(DateTime dateTime, int slaveID, float initTime):base(dateTime, slaveID)
        {
            LastUpdateTime = initTime;
       
            CargoOffered_A = new CargoHandlerModel();
            Accepted_A = false;
        
            CargoOffered_B = new CargoHandlerModel();
            Accepted_B = false;
            Pending = true;    
        
        }
        
    }


    public class ShipShipTrade : Trade, IHasUIData
    {
        public string UIDisplayName { get { return "Trade Data"; } }

        public IShip ShipA { get; set; }//Direct references might be dangerous, but they simplify things, leaving them for now
        [UIProperty]
        public int ShipID_A { get { return ShipA.Id; } }
        
        
        public IShip ShipB { get; set; }
        [UIProperty]
        public int ShipID_B { get { return ShipB.Id; } }
        

        public ShipShipTrade(IShip a, IShip b, int transactionID, DateTime dateTime, int slaveID, float initTime):base(dateTime, slaveID, initTime)
        {
            ShipA = a;
            ShipB = b;
            Id = transactionID;
        }

        public ShipShipTradeData GetNetworkData()
        {
            return new ShipShipTradeData
            {
                ShipA = new TradeData
                {
                    Accepted = Accepted_A,
                    CargoData = CargoOffered_A.GetNetworkData(),
                    ShipID = ShipA.Id
                },

                ShipB = new TradeData
                {
                    Accepted = Accepted_B,
                    CargoData = CargoOffered_B.GetNetworkData(),
                    ShipID = ShipB.Id
                }
            };
            
        }

    }   

}
