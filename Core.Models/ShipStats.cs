using Freecon.Models.TypeEnums;
using Core.Models.Enums;
using Freecon.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Models;
using Freecon.Models.UI;

namespace Core.Models
{
    /// <summary>
    /// Used for on-the-fly ship stat customization.
    /// Also writes itselfs to messages, saving lines of code and preventing breaking.
    /// </summary>
    public class ShipStats : ISerializable, IDBObject, IHasUIData
    {
        public string UIDisplayName { get { return ShipType.ToString().SplitCamelCase(); } }

        [UIProperty()]
        public ShieldTypes ShieldType = ShieldTypes.QuickRegen;

        
        [UIProperty()]
        public ShipTypes ShipType;

        public int Id { get; set; }

        [BsonId]
        [UIProperty()]
        public string Name { get; set; }
        [UIProperty()]
        public string Description { get; set; }
        public bool PlayerShip { get; set; }

        // ints
        [UIProperty(DisplayName="Max Shields", Units="TW")]
        public int MaxShields { get; set; }
        [UIProperty(DisplayName="Max Health")]
        public int MaxHealth { get; set; }
        [UIProperty(Units = "TW")]
        public int Energy { get; set; }
        [UIProperty(DisplayName = "Max Holds")]
        public int MaxHolds { get; set; }
        [UIProperty(DisplayName = "Base Weight", Units="kg")]
        public int BaseWeight { get; set; }

        // strings
        public ShipTextures Graphic { get; set; }
        public string ThrustGraphic { get; set; }
        [UIProperty()]
        public string Class { get; set; }

        // floats
        [UIProperty(DisplayName = "Turn Rate", Units="rad/s")]
        public float TurnRate { get; set; }

        [UIProperty(DisplayName = "Energy Regeneration Rate", Units="TW/s")]
        public float EnergyRegenRate { get; set; }

        [UIProperty(DisplayName = "Halo Shield Regeneration Rate", Units="TW/s")]
        public float HaloShieldRegenRate { get; set; }

        [UIProperty(DisplayName = "Slow Shield Regeneration Rate", Units="TW/s")]
        public float SlowShieldRegenRate { get; set; }

        [UIProperty(DisplayName = "Boost Bonus", Units="%")]
        public float BoostBonus { get; set; }

        [UIProperty(DisplayName = "Top Speed", Units="km/s")]
        public float TopSpeed { get; set; }

        [UIProperty(DisplayName = "Forward Thrust", Units="MN")]
        public float BaseThrustForward { get; set; }

        [UIProperty(DisplayName = "Reverse Thrust", Units = "MN")]
        public float BaseThrustReverse { get; set; }

        [UIProperty(DisplayName = "Lateral Thrust", Units = "MN")]
        public float BaseThrustLateral { get; set; }

        public ModelTypes ModelType { get { return ModelTypes.ShipStats; } }



        public ShipStats()
        { }

        public ShipStats(ShipStatData data)
        {
            ShieldType = data.ShieldType;
            ShipType = data.ShipType;
            Id = data.Id;
            Name = data.Name;
            Description = data.Description;
            MaxShields = data.MaxShields;
            MaxHealth = data.MaxHealth;
            Energy = data.Energy;
            MaxHolds = data.TotalHolds;
            BaseWeight = data.BaseWeight;
            Graphic = data.Graphic;
            ThrustGraphic = data.ThrustGraphic;
            Class = data.Class;
            TurnRate = data.TurnRate;
            EnergyRegenRate = data.EnergyRegenRate;
            HaloShieldRegenRate = data.HaloShieldRegenRate;
            SlowShieldRegenRate = data.SlowShieldRegenRate;
            BoostBonus = data.BoostBonus;
            TopSpeed = data.TopSpeed;
            BaseThrustForward = data.BaseThrustForward;
            BaseThrustReverse = data.BaseThrustReverse;
            BaseThrustLateral = data.BaseThrustLateral;

        }

        public ShipStatData GetNetworkData()
        {
            ShipStatData data = new ShipStatData();
            data.ShieldType = ShieldType;
            data.ShipType = ShipType;
            data.Id = Id;
            data.Name = Name;
            data.Description = Description;
            data.MaxShields = MaxShields;
            data.MaxHealth = MaxHealth;
            data.Energy = Energy;
            data.TotalHolds = MaxHolds;
            data.BaseWeight = BaseWeight;
            data.Graphic = Graphic;
            data.ThrustGraphic = ThrustGraphic;
            data.Class = Class;
            data.TurnRate = TurnRate;
            data.EnergyRegenRate = EnergyRegenRate;
            data.HaloShieldRegenRate = HaloShieldRegenRate;
            data.SlowShieldRegenRate = SlowShieldRegenRate;
            data.BoostBonus = BoostBonus;
            data.TopSpeed = TopSpeed;
            data.BaseThrustForward = BaseThrustForward;
            data.BaseThrustReverse = BaseThrustReverse;
            data.BaseThrustLateral = BaseThrustLateral;
            return data;
        }
             

        public IDBObject GetDBObject()
        {
            return this;
        }
    }

}