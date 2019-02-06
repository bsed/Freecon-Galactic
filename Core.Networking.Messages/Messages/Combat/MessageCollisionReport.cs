using Freecon.Models.TypeEnums;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageProjectileCollisionReport:MessagePackSerializableObject
    {
        public List<CollisionData> Collisions { get; set; }

        public MessageProjectileCollisionReport()
        {
            Collisions = new List<CollisionData>();
        }

    }

    public class CollisionData
    {
        public int HitObjectID { get; set; }
        public int CollisionID { get; set; }
        public int ProjectileID { get; set; }
        public ProjectileTypes ProjectileType { get; set; }
        public byte PctCharge { get; set; }
        public byte WeaponSlot { get; set; }
    
    }
}
