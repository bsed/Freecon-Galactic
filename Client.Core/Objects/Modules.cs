//using System;

//using Microsoft.Xna.Framework.Graphics;
//using Client.Objects.Ships;
//using Freecon.Models.TypeEnums;

//namespace Client.Objects
//{
//    public class Module
//    {
//        public Ship myShip;
//        public int size;
//        public Texture2D texture;

//        public byte tier;
//        public byte type;
//        public int weight;

//        public Module(byte tier)
//        {
//            this.tier = tier;
//        }

//        public virtual void addToBonusStat()
//        {
//        }

//        public virtual void removeFromBonusStat()
//        {
//        }
//    }


//    public class EnergyRegenModule : Module
//    {
//        private int rateBonus = 10;

//        public EnergyRegenModule(byte tier)
//            : base(tier)
//        {
//            type = (byte) ModuleTypes.EnergyRegenModule;
//        }

//        public override void addToBonusStat()
//        {
//            myShip.energyRegenBonus += rateBonus;
//        }

//        public override void removeFromBonusStat()
//        {
//            myShip.energyRegenBonus -= rateBonus;
//        }
//    }

//    public class ThrustModule : Module
//    {
//        private int thrustBonus = 10;

//        public ThrustModule(byte tier)
//            : base(tier)
//        {
//            type = (byte) ModuleTypes.ThrustModule;
//        }

//        public override void addToBonusStat()
//        {
//            myShip.thrustBonus += thrustBonus;
//        }

//        public override void removeFromBonusStat()
//        {
//            myShip.thrustBonus -= thrustBonus;
//        }
//    }

//    public class MaxShieldModule : Module
//    {
//        private UInt16 shieldBonus = 100;

//        public MaxShieldModule(byte tier)
//            : base(tier)
//        {
//            type = (byte) ModuleTypes.ThrustModule;
//        }

//        public override void addToBonusStat()
//        {
//            myShip.maxShieldsBonus += shieldBonus;
//        }

//        public override void removeFromBonusStat()
//        {
//            myShip.maxShieldsBonus -= shieldBonus;
//        }
//    }

//    public class ShieldRegenModule : Module
//    {
//        public ShieldRegenModule(byte tier)
//            : base(tier)
//        {
//            type = (byte) ModuleTypes.ShieldRegenModule;
//        }
//    }

//    public class MaxEnergyModule : Module
//    {
//        private int maxBonus = 10;

//        public MaxEnergyModule(byte tier)
//            : base(tier)
//        {
//            type = (byte) ModuleTypes.MaxEnergyModule;
//        }

//        public override void addToBonusStat()
//        {
//            myShip.maxEnergyBonus += maxBonus;
//            myShip.createDictionaries((int)myShip.ShipStats.Energy + (int)myShip.maxEnergyBonus);
//                //Obfuscation dictionaries need to be updated
     
//        }

//        public override void removeFromBonusStat()
//        {
//            myShip.maxEnergyBonus -= maxBonus;
//            myShip.createDictionaries((int)myShip.ShipStats.Energy + (int)myShip.maxEnergyBonus);
//                //Obfuscation dictionaries need to be updated
 
//        }
//    }
//}