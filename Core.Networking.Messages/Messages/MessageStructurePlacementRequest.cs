using Freecon.Models.TypeEnums;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageStructurePlacementRequest : MessagePackSerializableObject
    {
        /// <summary>
        /// Optional, the ID of the cargo object if this is a structure deployed from cargo (e.g. a turret)
        /// </summary>
        public int? CargoID { get; set; }  
        public int RequestingShipID { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public StructureTypes StructureType { get; set; }

        public MessageStructurePlacementRequest()
        {
        }

        public MessageStructurePlacementRequest(int requestingShipID, float posX, float posY, StructureTypes structureType)
        {
            PosX = posX;
            PosY = posY;
            StructureType = structureType;
            RequestingShipID = requestingShipID;
        }
    }
}
