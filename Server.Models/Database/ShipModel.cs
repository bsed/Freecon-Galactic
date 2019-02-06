using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using MongoDB.Bson.Serialization.Attributes;
using Server.MongoDB;
using Core.Models;
using Core.Models.CargoHandlers;
using Freecon.Core.Interfaces;
using Freecon.Models;


namespace Server.Models
{

    public class ShipModel: IDBObject
    {
        [BsonId(IdGenerator = typeof(GalaxyIDIDGenerator))]
        public int Id { get; set; }               

        public float AngVel { get; set; }
        public float CurrentEnergy { get; set; }
        public float CurrentHealth { get; set; }
        public float CurrentShields { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Rotation { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }

        public bool IsNPC { get; set; }

        public int? CurrentAreaID { get; set; }

        public int? PlayerID { get; set; }

        public List<Weapon> Weapons { get; set; }      
        public int MissileLauncherSlot { get; set; } 
        
        public ShipStats ShipStats { get; set; }

        public PilotTypes PilotType { get; set; }

        public ShipTypes ShipType { get; set; }      

        public List<Module> Modules { get; set; }

        public CargoHandlerModel Cargo { get; set; }

        public float TotalHolds{ get; set; }

        public ModelTypes ModelType { get { return ModelTypes.ShipModel; } }

        public ShipModel()
        {
            Weapons = new List<Weapon>();
            Modules = new List<Module>();
        }
            
        public ShipModel(ShipModel s)
        {
            Id = s.Id;
            AngVel = s.AngVel;
            CurrentEnergy = s.CurrentEnergy;
            CurrentHealth = s.CurrentHealth;
            CurrentShields = s.CurrentShields;
            PosX = s.PosX;
            PosY = s.PosY;
            Rotation = s.Rotation;
            VelX = s.VelX;
            VelY = s.VelY;
            CurrentAreaID = s.CurrentAreaID;

            PlayerID = s.PlayerID;
            Weapons = s.Weapons;
            ShipStats = s.ShipStats;
            PilotType = s.PilotType;

            IsNPC = s.IsNPC;

            Modules = s.Modules;

            Cargo = s.Cargo;

            TotalHolds = s.TotalHolds;
        }
        
        public ShipModel GetClone()
        {
            return (ShipModel)MemberwiseClone();
        }

    }

    
    
}
