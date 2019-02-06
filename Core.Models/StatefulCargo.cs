using Freecon.Core.Networking.Models.Objects;
using Freecon.Models.TypeEnums;
using System;
using Freecon.Core.Interfaces;
using Freecon.Models;
using Freecon.Models.UI;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Models
{
    public class StatefulCargo_RO: IHasGalaxyID, IHasUIData
    {
        [BsonElement]
        protected internal ModelStatefulCargo _model { get; set; }

        public virtual string UIDisplayName { get { return CargoType.ToString().SplitCamelCase(); } }

        [UIProperty(IsDisplayed=false)]
        public StatefulCargoTypes CargoType { get { return _model.Type; }}

        [BsonIgnore]
        [UIProperty(IsDisplayed=false)]//Needed to be able to select the cargo
        public virtual int Id { get { return _model.Id; } protected set { _model.Id = value; } }

        public StatefulCargo_RO()
        {
            _model = new ModelStatefulCargo();
        }

        public StatefulCargo_RO(StatefulCargo c)
        {
            _model = c._model;
        }
       

        /// <summary>
        /// Returns the number of holds each cargo object occupies
        /// </summary>
        public static float SpacePerObject(StatefulCargoTypes type)
        {
            switch (type)
            {
                case StatefulCargoTypes.LaserTurret:
                    return 2f;

                case StatefulCargoTypes.Laser:
                    return .5f;

                case StatefulCargoTypes.Barge:
                    return 25000f;

                case StatefulCargoTypes.Reaper:
                    return 15000f;

                case StatefulCargoTypes.BattleCruiser:
                    return 20000f;

                case StatefulCargoTypes.Penguin:
                    return 12000f;

                case StatefulCargoTypes.Module:
                    return 4f;

                case StatefulCargoTypes.DefensiveMine:
                    return 2f;

                default:
                    Console.WriteLine("SpacePerObject not implemented for this cargo of type " + type.ToString());
                    return 1;

            }



        }

        /// <summary>
        /// Writes stats to message.
        /// </summary>
        /// <param name="msg"></param>
        public virtual StatefulCargoData GetNetworkObject()
        {
            StatefulCargoData data = new StatefulCargoData();
            data.CargoType = CargoType;
            data.Id = Id;
            return data;
        }

    }

    public class StatefulCargo : StatefulCargo_RO
    {
        [BsonIgnore]
        public new StatefulCargoTypes CargoType { get { return _model.Type; } 
            set { _model.Type = value; } }

        /// <summary>
        /// Just in case we decide to let ports store cargo that isn't for sale.
        /// Prevents an exploit where a "smart" user could send a request for an item that isn't displayed but is in the cargo
        /// </summary>
        public bool IsForSale { get; set; }

        public StatefulCargo():base()
        {
            IsForSale = true;
        }

        public StatefulCargo(int ID, StatefulCargoTypes type)
        {
            IsForSale = true;
            _model.Id = ID;
            CargoType = type;
        }

        
        public void SetID(int newID)
        {
            _model.Id = newID;
        }
                
    }

    /// <summary>
    /// Excessive, but ensures that a read only StatefulCargo can exist without duplicating a unique GalaxyID
    /// </summary>
    public class ModelStatefulCargo
    {
        public StatefulCargoTypes Type;
                

        public int Id { get; set; }

    }

    public class CargoShip : StatefulCargo
    {
        //TODO: I think this is excessive, need to just use WeaponType enum and implement a static dictionary/helper to get a WeaponStats instance as necessary
        //But for now, useful for the port ui
        [UICollection(DisplayName = "Stats")]
        public ShipStats ShipStats;

        public CargoShip(int ID, float health, ShipStats shipStats)
            : base(ID, StatefulCargoTypes.Null)
        {
            ShipStats = shipStats;
            CargoType = (StatefulCargoTypes)Enum.Parse(typeof(StatefulCargoTypes), shipStats.ShipType.ToString(), true);//Gross, I know, but we need to get some kind of port ui working already, can fix later.//TODO: fix...
        }


    }

    public class CargoWeapon : StatefulCargo
    {
        //TODO: I think this is excessive, need to just use WeaponType enum and implement a static dictionary/helper to get a WeaponStats instance as necessary
        //But for now, useful for the port ui
        [UICollection(DisplayName = "Stats")]
        public WeaponStats WeaponStats;

        public CargoWeapon(int ID, float health, WeaponStats weaponStats)
            : base(ID, StatefulCargoTypes.Laser)
        {
            WeaponStats = weaponStats;
            CargoType = (StatefulCargoTypes)Enum.Parse(typeof(StatefulCargoTypes), weaponStats.WeaponType.ToString(), true);//Gross, I know, but we need to get some kind of port ui working already, can fix later.//TODO: fix...
        }

    }
    
    public class CargoLaserTurret : StatefulCargo
    {
        [UIProperty]
        public float Health;

        //TODO: I think this is excessive, need to just use WeaponType enum and implement a static dictionary/helper to get a WeaponStats instance as necessary
        //But for now, useful for the port ui
        [UICollection(DisplayName="Stats")]
        public WeaponStats WeaponStats { get; protected set; }

        public CargoLaserTurret()
        {
            
        }

        public CargoLaserTurret(int ID, float health, WeaponStats weaponStats)
            : base(ID, StatefulCargoTypes.LaserTurret)
        {
            Health = health;
            WeaponStats = weaponStats;
        }

        /// <summary>
        /// Writes stats to message.
        /// </summary>
        /// <param name="msg"></param>
        public override StatefulCargoData GetNetworkObject()
        {
            CargoLaserTurretData data = new CargoLaserTurretData();
            data.CargoType = CargoType;
            data.Health = Health;
            data.Id = Id;
            return data;
        }
    }
}
