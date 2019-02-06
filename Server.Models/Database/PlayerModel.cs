using System;
using System.Collections.Generic;
using Freecon.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using Server.MongoDB;


namespace Server.Models
{

    public class PlayerModel : IDBObject
    {
        
        [BsonId(IdGenerator = typeof(GalaxyIDIDGenerator))]
        public int Id { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.PlayerModel; } }


        public int? AccountID { get; set; }

        public float CashInBank { get; set; }
        public float CashOnHand { get; set; }

        public int? ActiveShipId { get; set; } 

        public HashSet<int> OwnedShipIds { get; set; }

        public int HackCount { get; set; }

        public int PlayersKilled { get; set; } 

        public string Username { get; set; }

        public RonBurgundy NewsManager { get; set; }

        public TimeSpan clientTime { get; set; }

        public int? CurrentAreaID { get; set; }

        public bool IsOnline { get; set; }

        public long LastHeartbeat { get; set; }
        public DateTime ServerLastTime { get; set; }

        public HashSet<int> TeamIDs { get; set; }

        public int DefaultTeamID { get; set; }//Every player gets a unique default teamID on creation, for things like structures

        public HashSet<int> ColonizedPlanetIDs { get; set; }

        public HashSet<int> SimulatingShipIDs { get; set; }

        public bool IsHandedOff { get; set; }//Set to true after a handoff to avoid logout on disconnection

        public PlayerTypes PlayerType { get; set; }

        public PlayerModel()
        {
            ColonizedPlanetIDs = new HashSet<int>();
            SimulatingShipIDs = new HashSet<int>();
            TeamIDs = new HashSet<int>();
            PlayersKilled = 0;
            CashOnHand = 0;
            CashInBank = 0;
            OwnedShipIds = new HashSet<int>();
        }

        public PlayerModel GetClone()
        {
            return (PlayerModel)MemberwiseClone();
        }
    }
}
