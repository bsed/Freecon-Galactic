using Core.Models;
using Freecon.Models.TypeEnums;
using Core.Models.Enums;

namespace Freecon.Models
{
    public abstract class ShieldHandler   
    {
        public abstract ShieldTypes ShieldType { get; }
        public virtual float CurrentShields { get; set; }

        public float TimeOfLastDamage { get; set; }

        public ShipBonusHandler BonusHandler { get; protected set; }

        public DebuffHandler Debuffs { get; protected set; }

        public ShipStats ShipStats;

        public virtual void Update(float currentTimeMS)
        {
            _lastTimeStamp = currentTimeMS;
        }

        protected float _lastTimeStamp;


        public ShieldHandler(ShipStats shipStats, ShipBonusHandler shipBonusHandler, DebuffHandler debuffHandler)
        {
            BonusHandler = shipBonusHandler;
            Debuffs = debuffHandler;
            ShipStats = shipStats;
        }

    }


    public class QuickRegenShieldHandler:ShieldHandler
    {
        public override ShieldTypes ShieldType { get { return ShieldTypes.QuickRegen; } }

        public QuickRegenShieldHandler(ShipStats shipStats, ShipBonusHandler shipBonusHandler, DebuffHandler debuffHandler)
            : base(shipStats, shipBonusHandler, debuffHandler)
        { }

        public override void Update(float currentTimeMS)
        {
            base.Update(currentTimeMS);


            float shieldRate = ShipStats.HaloShieldRegenRate * BonusHandler[StatBonusTypes.ShieldRegen] / (1 + Debuffs[DebuffTypes.ShieldRegen]);
            float shieldChangeAmount = shieldRate * (currentTimeMS - _lastTimeStamp);

            if (currentTimeMS - TimeOfLastDamage >= 3000) //Shields are recharging
            {
                if (CurrentShields + shieldChangeAmount >= (ShipStats.MaxShields + BonusHandler[StatBonusTypes.MaxShields]))
                {
                    CurrentShields = (ShipStats.MaxShields + BonusHandler[StatBonusTypes.MaxShields]);
                }
                else
                {
                    CurrentShields += shieldChangeAmount;
                }
            }
        }
    }

    public class SlowRegenShieldHandler:ShieldHandler
    {
        public override ShieldTypes ShieldType { get { return ShieldTypes.SlowRegen; } }

        public SlowRegenShieldHandler(ShipStats shipStats, ShipBonusHandler shipBonusHandler, DebuffHandler debuffHandler)
            : base(shipStats, shipBonusHandler, debuffHandler)
        { }

        public override void Update(float currentTimeMS)
        {
            base.Update(currentTimeMS);

            float shieldRate = ShipStats.SlowShieldRegenRate * BonusHandler[StatBonusTypes.ShieldRegen] / (1 + Debuffs[DebuffTypes.ShieldRegen]);
            float shieldChangeAmount = shieldRate * (currentTimeMS - _lastTimeStamp);

            if (CurrentShields == 0)
            {
                if (currentTimeMS - TimeOfLastDamage >= 6000)
                    //Shields can start recharging
                    CurrentShields += (int)(shieldChangeAmount);
                else
                {
                }
            }
            else if (CurrentShields + shieldChangeAmount >= (ShipStats.MaxShields + BonusHandler[StatBonusTypes.MaxShields]))
            {
                CurrentShields = (ShipStats.MaxShields + BonusHandler[StatBonusTypes.MaxShields]);
            }
            else
            {
                CurrentShields += shieldChangeAmount;
            }
        }
        
        
    }

    public class NoRegenShieldHandler : ShieldHandler
    {
        public override ShieldTypes ShieldType { get { return ShieldTypes.NoRegen; } }

        public NoRegenShieldHandler(ShipStats shipStats, ShipBonusHandler shipBonusHandler, DebuffHandler debuffHandler)
            : base(shipStats, shipBonusHandler, debuffHandler)
        { }

        public override void Update(float currentTimeMS)
        {
            base.Update(currentTimeMS);
        }

    }


}
