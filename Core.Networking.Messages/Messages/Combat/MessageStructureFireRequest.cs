using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageStructureFireRequest:MessagePackSerializableObject
    {
        public int StructureID { get; set; }

        public float Rotation { get; set; }

        public byte WeaponSlot { get; set; }

        //Although IDs must be unique, don't change to HashSet, because order should be preserved between clients.
        public List<int> ProjectileIDs { get; set; }

        public byte PctCharge { get; set; }

        public MessageStructureFireRequest()
        {
            ProjectileIDs = new List<int>();
        }
    }
}
