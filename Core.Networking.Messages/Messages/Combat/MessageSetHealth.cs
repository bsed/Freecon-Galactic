using System.Collections.Generic;
using Core.Models.Enums;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageSetHealth:MessagePackSerializableObject
    {
        /// <summary>
        /// Pre-optimized for batch submission, which is not implemented as of this writing
        /// </summary>
        public List<HealthData> HealthData { get; set; }
        
        public MessageSetHealth()
        {
            HealthData = new List<HealthData>();
        }

    }

    public class HealthData
    {
        public int ShipID {get;set;}
        public int Health { get; set; }
        public int Shields { get; set; }
        public int Energy { get; set; }

        /// <summary>
        /// Not sure if MessagePack can handle dictionaries, so keep DebuffTypes and DebuffCounts index-synchronized
        /// </summary>
        public List<DebuffTypes> DebuffTypesToAdd { get; set; }

        /// <summary>
        /// Not sure if MessagePack can handle dictionaries, so keep DebuffTypes and DebuffCounts index-synchronized
        /// //TODO: Test MessagePack dictionary serialization
        /// </summary>
        public List<int> DebuffCountsToAdd { get; set; }

        public HealthData()
        {
            DebuffTypesToAdd = new List<DebuffTypes>();
            DebuffCountsToAdd = new List<int>();
        }
         
    }

}
