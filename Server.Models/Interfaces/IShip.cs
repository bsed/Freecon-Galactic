using Core.Models;
using Core.Models.CargoHandlers;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Models;
using Freecon.Models.TypeEnums;
using Server.Models;
using Server.Models.Interfaces;
using System.Collections.Generic;
using Freecon.Core.Interfaces;

namespace Server.Interfaces
{
    public interface IShip : ICollidable, IHasGalaxyID, ISimulatable, ICanFire, ISerializable, IKillable, ITeamable, IHasCargo, IHasPosition, IHasUIData
    {
        float AngVel { get; set; }
        
        CargoHandler_ReadOnlyVM<CargoHandlerModel> Cargo { get; }
        
        void ChangeEnergy(float amount);
        
        void ChangeShipType(ShipStats shs);
        

        float CurrentEnergy { get; set; }
        
        float CurrentHealth { get; set; }

        float MaxHealth { get; }
        float MaxShields { get; }
        float MaxEnergy { get; }
        
        ShieldHandler Shields { get; }

        /// <summary>
        /// Area to send ship to on revival
        /// </summary>
        int? ReviveAreaID { get; set; }

        int Id { get; set; }
        
        bool Thrusting { get; set; }

        float LastWarpTime { get; set; }

        IArea GetArea();
        
        Player GetPlayer();
        
        Player GetSimulatingPlayer();
        
        bool IsDead { get; set; }

        float KillTimeStamp { get; set; }

        float LastHealthUpdateTime { get; set; }

        float HealthUpdatePeriod { get; set; }

        float RespawnTimeDelay { get; set; }
        
        PilotTypes PilotType { get; }
        
        int? PlayerID { get; }
        
        float PosX { get; set; }
        
        float PosY { get; set; }

        float Rotation { get; set; }

        Weapon GetWeapon(int slot);

        /// <summary>
        /// If slot==-1, weapon appended to the end of the weapon list.
        /// </summary>
        /// <param name="weaponType"></param>
        /// <param name="slot"></param>
        void SetWeapon(Weapon weapon, int slot = -1);
        
        IShip SetArea(IArea newArea);
        
        IShip SetPlayer(Player p);
        
        ShipStats ShipStats { get; set; }
        
        ShipBonusHandler StatBonuses { get; }

        DebuffHandler Debuffs { get; }
        
        void Update();
                
        float VelX { get; set; }
        
        float VelY { get; set; }

        ShipData GetNetworkData(bool writeTeams, bool writeMods, bool writeCargo, bool writeStats = false);

        List<int> GetModuleIDs();

        void CargoAdded(object sender, ITransactionAddStatefulCargo transaction);

        void CargoRemoved(object sender, ITransactionRemoveStatefulCargo transaction);

    }
}
