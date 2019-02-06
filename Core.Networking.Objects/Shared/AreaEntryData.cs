using System.Collections.Generic;
using MsgPack.Serialization;

namespace Freecon.Core.Networking.Models.Objects
{
    public class AreaEntryData : MessagePackSerializableObject
    {
        public string AreaName;
        public int AreaSize;
        public int Id;

        public float NewPlayerXPos;
        public float NewPlayerYPos;

        public byte SecurityLevel { get; set; }

        public List<WarpholeData> Warpholes;


        [MessagePackRuntimeCollectionItemType]
        public List<StructureData> Structures;

        public List<FloatyAreaObjectData> FloatyAreaObjects;

        public List<ShipData> Ships { get; set; }

        public AreaEntryData()
        {
            Warpholes = new List<WarpholeData>();
            Structures = new List<StructureData>();
            FloatyAreaObjects = new List<FloatyAreaObjectData>();
            Ships = new List<ShipData>();

        }

        public AreaEntryData(AreaEntryData a)
        {
            AreaName = a.AreaName;
            AreaSize = a.AreaSize;
            Id = a.Id;
            NewPlayerXPos = a.NewPlayerXPos;
            NewPlayerYPos = a.NewPlayerYPos;
            SecurityLevel = a.SecurityLevel;
            Warpholes = a.Warpholes;
            Structures = a.Structures;
            FloatyAreaObjects = a.FloatyAreaObjects;
            Ships = a.Ships;

        }

    }
}