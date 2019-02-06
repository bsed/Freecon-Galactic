using System.Collections.Generic;

namespace Freecon.Models
{
    public class ShipBonusHandler
    {
        ShipBonusHandlerModel _model;
        public float this[StatBonusTypes type] { get { return _model[type]; } }

        public ShipBonusHandler()
        {
            _model = new ShipBonusHandlerModel();

        }

        public void AddBonus(Module m)
        {
            m.AddToBonusStat(_model);
        }

        public void RemoveBonus(Module m)
        {
            m.RemoveFromBonusStat(_model);
        }

        /// <summary>
        /// Resets all bonuses to their default values
        /// </summary>
        public void Reset()
        {
            _model.Reset();
        }

    }


    public class ShipBonusHandlerModel
    {
        Dictionary<StatBonusTypes, float> Bonuses { get; set; }

        public float this[StatBonusTypes type] { get { return Bonuses[type]; } set { Bonuses[type] = value; } }


        public int Id { get; set; }//For DB
               

        public ShipBonusHandlerModel()
        {
            Bonuses = new Dictionary<StatBonusTypes, float>();
            
            Bonuses.Add(StatBonusTypes.MaxEnergy, 0);
            Bonuses.Add(StatBonusTypes.MaxHealth, 0);
            Bonuses.Add(StatBonusTypes.MaxShields, 0);
            Bonuses.Add(StatBonusTypes.EnergyRegen, 1);
            Bonuses.Add(StatBonusTypes.ShieldRegen, 1);
            Bonuses.Add(StatBonusTypes.Thrust, 1);
            Bonuses.Add(StatBonusTypes.TopSpeed, 0);
            Bonuses.Add(StatBonusTypes.TurnRate, 1);
            Bonuses.Add(StatBonusTypes.LateralThrust, 1);
            Bonuses.Add(StatBonusTypes.Damage, 1);
            Bonuses.Add(StatBonusTypes.Defense, 1);


            Reset();
        }

        public void Reset()
        {
            Bonuses[StatBonusTypes.MaxEnergy]=0;
            Bonuses[StatBonusTypes.MaxHealth]=0;
            Bonuses[StatBonusTypes.MaxShields]=0;
            Bonuses[StatBonusTypes.EnergyRegen]=1;
            Bonuses[StatBonusTypes.ShieldRegen]=1;
            Bonuses[StatBonusTypes.Thrust]=1;
            Bonuses[StatBonusTypes.TopSpeed]=0;
            Bonuses[StatBonusTypes.TurnRate]=1;
            Bonuses[StatBonusTypes.LateralThrust]=1;
            Bonuses[StatBonusTypes.Damage]=1;
            Bonuses[StatBonusTypes.Defense] = 1;
            
        }

    }

    public enum StatBonusTypes:byte
    {
        MaxEnergy,
        MaxHealth,
        MaxShields,
        EnergyRegen,
        ShieldRegen,
        Thrust,
        TopSpeed,
        TurnRate,
        LateralThrust,
        Damage,
        Defense,

    }
}
