using System.Collections.Generic;
using Freecon.Models.TypeEnums;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageShipFireRequest:MessagePackSerializableObject
    {
        public int ShipID { get; set; }
        public float Rotation { get; set; }
        public byte WeaponSlot { get; set; }
        public byte PctCharge { get; set; }
        public ProjectileTypes ProjectileType { get; set; }

        //Although IDs must be unique, don't change to HashSet, because order should be preserved between clients.
        public List<int> ProjectileIDs { get; set; }

        public MessageShipFireRequest()
        {
            ProjectileIDs = new List<int>();
        }
    }
}
