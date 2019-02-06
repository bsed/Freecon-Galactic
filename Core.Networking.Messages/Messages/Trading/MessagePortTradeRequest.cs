using Freecon.Core.Models.Enums;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessagePortTradeRequest : MessagePackSerializableObject
    {
        public int ShipID { get; set; }

        public PortTradeDirection TradeDirection { get; set; }

        /// <summary>
        /// Leave null if unused
        /// </summary>
        public Dictionary<PortWareIdentifier, float> TypesAndQuantities { get; set; }
        

        /// <summary>
        /// Leave null if unused
        /// </summary>
        public HashSet<int> StatefulCargoIDs { get; set; }

    }

    
}
