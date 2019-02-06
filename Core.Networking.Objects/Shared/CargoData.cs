using Freecon.Models.TypeEnums;
using MsgPack.Serialization;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Objects
{
    public class CargoData
    {
        public float TotalHolds { get; set; }

        public float FilledHolds { get; set; }

        public HashSet<StatelessCargoData> StatelessCargo { get; set; }

        [MessagePackRuntimeCollectionItemType]
        public HashSet<StatefulCargoData> StatefulCargo { get; set;}

       

        public CargoData()
        {
            StatelessCargo = new HashSet<StatelessCargoData>();
            StatefulCargo = new HashSet<StatefulCargoData>();
        }        
    }

    public class StatelessCargoData
    {
        public StatelessCargoTypes CargoType { get; set; }
        public float Quantity { get; set; }
    }



    public class StatefulCargoData
    {
        public StatefulCargoTypes CargoType { get; set; }

        public int Id { get; set; }

    }

    public class CargoLaserTurretData : StatefulCargoData
    {
        public float Health { get; set; }
    }

    public class CargoModuleData : StatefulCargoData
    {
        public ModuleTypes ModuleType { get; set; }
        public byte Level { get; set; }

        public CargoModuleData(ModuleTypes moduleType, byte level)
        {
            CargoType = StatefulCargoTypes.Module;
            ModuleType = moduleType;
            Level = level;
        }
    }
}
