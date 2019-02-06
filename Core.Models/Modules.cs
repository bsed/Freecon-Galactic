using Core.Models;
using Core.Models.Enums;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Models.TypeEnums;
using Freecon.Models.UI;
using MongoDB.Bson.Serialization.Attributes;

namespace Freecon.Models
{
    public abstract class Module : StatefulCargo, IFloatyAreaObject, IHasUIData
    {
        public override string UIDisplayName
        {
            get
            {
                var n = GetType().ToString().Split(new char[] { '.' });
                return n[n.Length - 1].SplitCamelCase();
            }
        }

        public float PosX { get; set; }

        public float PosY { get; set; }

        public float Rotation { get; set; }

        /// <summary>
        /// Used to transfer module to the appropriate area when a ship is killed. Hacky but convenient
        /// </summary>
        [BsonIgnore]
        public int NextAreaID { get; set; }

        [UIProperty(IsDisplayed=false)]
        public override int Id { get { return base.Id; } protected set { base.Id = value; } }

        [UIProperty]
        public byte Level { get; set; }

        [UIProperty]
        public abstract ModuleTypes ModuleType { get; }

        /// <summary>
        /// Strictly for the UI.
        /// </summary>
        /// <value>
        /// The type of the modifier.
        /// </value>
        [UIProperty]
        public abstract ModifierType ModifierType { get; }

        public FloatyAreaObjectTypes FloatyType { get { return FloatyAreaObjectTypes.Module; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// NOTE: If you're getting a funky null refence exception error when deserializing a module,
        /// ensure that there is a public or protected, parameterless constructor in the module class.
        /// </summary>
        protected Module()
        {

        }

        public Module(int galaxyID, byte level)
        {
            Id = galaxyID;
            Level = level;
            _model.Type = StatefulCargoTypes.Module;
        }

        public virtual void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {


        }

        public virtual void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
        }

        public virtual FloatyAreaObjectData GetFloatyNetworkData()
        {
            FloatyAreaObjectData data = new FloatyAreaObjectData();
            data.FloatyType = FloatyType;
            data.Id = Id;
            data.Rotation = Rotation;
            data.XPos = PosX;
            data.YPos = PosY;
            return data;
        }

        public override StatefulCargoData GetNetworkObject()
        {
            CargoModuleData data = new CargoModuleData(ModuleType, Level);
            data.CargoType = CargoType;
            data.Id = Id;
            return data;
        }
    }

    public class EnergyRegenModule : Module
    {
        [UIProperty(DisplayName="Rate Bonus")]
        private float _rateBonus = .1f;

        //Helpful tip for future devs: attributes are retained in overridden properties. Saved you a google search.
        public override ModuleTypes ModuleType { get { return ModuleTypes.EnergyRegenModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Multiplicative; } }

        protected EnergyRegenModule()
        { }

        public EnergyRegenModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.EnergyRegen] += _rateBonus * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.EnergyRegen] -= _rateBonus * Level;
        }

    }

    public class ThrustModule : Module
    {
        [UIProperty(DisplayName="Thrust Bonus", Units="%")]
        private float thrustBonus = .1f;

        public override ModuleTypes ModuleType { get { return ModuleTypes.ThrustModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Multiplicative; } }

        protected ThrustModule()
        {

        }

        public ThrustModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.Thrust] += thrustBonus * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.Thrust] -= thrustBonus * Level;
        }
                
    }
    public class LateralThrustModule : Module
    {
        [UIProperty(DisplayName = "Thrust Bonus", Units = "%")]
        private float _thrustBonus = .1f;

        public override ModuleTypes ModuleType { get { return ModuleTypes.LateralThrustModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Multiplicative; } }

        protected LateralThrustModule()
        {

        }

        public LateralThrustModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.LateralThrust] += _thrustBonus * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.LateralThrust] -= _thrustBonus * Level;
        }
       
    }

    public class MaxShieldModule : Module
    {
        [UIProperty(DisplayName = "Shield Bonus")]
        private int shieldBonus = 100;
        public override ModuleTypes ModuleType { get { return ModuleTypes.MaxShieldModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Additive; } }

        protected MaxShieldModule()
        { }

        public MaxShieldModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.MaxShields] += shieldBonus * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.MaxShields] -= shieldBonus * Level;
        }

    }

    public class ShieldRegenModule : Module
    {
        [UIProperty(DisplayName="Regeneration Rate Bonus", Units="TW/s")]
        float _regenBonusMultiplier = 1.1f;

        public override ModuleTypes ModuleType { get { return ModuleTypes.ShieldRegenModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Multiplicative; } }

        protected ShieldRegenModule()
        { }

        public ShieldRegenModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.ShieldRegen] += _regenBonusMultiplier * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.ShieldRegen] -= _regenBonusMultiplier * Level;
        }


    }

    public class MaxEnergyModule : Module
    {
        [UIProperty(DisplayName="Max Energy Bonus", Units="TJ")]
        private int maxBonus = 100;
        public override ModuleTypes ModuleType { get { return ModuleTypes.MaxEnergyModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Additive; } }

        protected MaxEnergyModule()
        { }

        public MaxEnergyModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.MaxEnergy] += maxBonus * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.MaxEnergy] -= maxBonus * Level;
        }
       
    }

    public class TurnRateModule : Module
    {
        [UIProperty(DisplayName="Turn Rate Bonus", Units="rad/s")]
        private float turnRateBonus = .05f;
        public override ModuleTypes ModuleType { get { return ModuleTypes.TurnRateModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Additive; } }

        protected TurnRateModule()
        { }

        public TurnRateModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.TurnRate] += turnRateBonus * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.TurnRate] -= turnRateBonus * Level;
        }
        
    }
    public class DamageModule : Module
    {
        [UIProperty(DisplayName="Damage Multiplier", Units="%")]
        private float _damageMultiplier = .1f;
        public override ModuleTypes ModuleType { get { return ModuleTypes.DamageModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Multiplicative; } }

        protected DamageModule()
        { }

        public DamageModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.Damage] += _damageMultiplier * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.Damage] -= _damageMultiplier * Level;
        }
       
    }

    public class DefenseModule : Module
    {
        [UIProperty(DisplayName="Damage Reduction", Units="%")]
        private float _defenseMultiplier = .1f;
        public override ModuleTypes ModuleType { get { return ModuleTypes.DefenseModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Multiplicative; } }

        protected DefenseModule()
        { }

        public DefenseModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.Defense] += _defenseMultiplier * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.Defense] -= _defenseMultiplier * Level;
        }
     
    }

    public class TopSpeedModule : Module
    {
        [UIProperty(DisplayName="Top Speed Multiplier", Units="%")]
        private float _multiplier = .1f;
        public override ModuleTypes ModuleType { get { return ModuleTypes.TopSpeedModule; } }

        public override ModifierType ModifierType { get { return ModifierType.Multiplicative; } }

        protected TopSpeedModule()
        { }

        public TopSpeedModule(int galaxyID, byte level)
            : base(galaxyID, level)
        {

        }

        public override void AddToBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.TopSpeed] += _multiplier * Level;
        }

        public override void RemoveFromBonusStat(ShipBonusHandlerModel bonusHandler)
        {
            bonusHandler[StatBonusTypes.TopSpeed] -= _multiplier * Level;
        }
      
    }

}
