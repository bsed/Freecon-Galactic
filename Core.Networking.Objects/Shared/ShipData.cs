using Freecon.Models.TypeEnums;
using System.Collections.Generic;
using Core.Models.Enums;

namespace Freecon.Core.Networking.Models.Objects
{
    public class ShipData:MessagePackSerializableObject
    {
        public ShipTypes ShipType { get; set; }
        public bool IsNPC { get; set; }
        public int? PlayerID { get; set; }
        public int Id { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float Rotation { get; set; }
        public string PlayerName { get; set; }
        public ShieldTypes ShieldType { get; set; }
        public List<WeaponTypes> WeaponTypes { get; set; }
        public ProjectileTypes SelectedMissileType { get; set; }//This will probably need tweaking later...
        public float CurrentHealth { get; set; }
        public float CurrentShields { get; set; }
        public bool IsDead { get; set; }

        public PilotTypes PilotType { get; set; }

        public ShipStatData ShipStats { get; set; }
        public HashSet<int> TeamIDs { get;set;}

        public CargoData Cargo { get; set; }

        public ShipData()
        {
            TeamIDs = new HashSet<int>();            
            WeaponTypes = new List<WeaponTypes>();
        }


    }

    public class ShipStatData//This is really gross redundant code, but I don't know of a convenient workaround without an ugly refactor
    {
        public ShieldTypes ShieldType { get; set; }
        public ShipTypes ShipType { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxShields { get; set; }
        public int MaxHealth { get; set; }
        public int Energy { get; set; }
        public int TotalHolds { get; set; }
        public int BaseWeight { get; set; }
        public ShipTextures Graphic { get; set; }
        public string ThrustGraphic { get; set; }
        public string Class { get; set; }
        public float TurnRate { get; set; }
        public float EnergyRegenRate { get; set; }
        public float HaloShieldRegenRate { get; set; }
        public float SlowShieldRegenRate { get; set; }
        public float BoostBonus { get; set; }
        public float TopSpeed { get; set; }
        public float BaseThrustForward { get; set; }
        public float BaseThrustReverse { get; set; }
        public float BaseThrustLateral { get; set; }
    }

    
    
}
