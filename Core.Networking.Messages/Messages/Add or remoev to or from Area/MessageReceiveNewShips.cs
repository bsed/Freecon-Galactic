using Freecon.Core.Networking.Models.Objects;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageReceiveNewShips:MessagePackSerializableObject
    {
        public List<ShipData> Ships { get; set; }

        public MessageReceiveNewShips()
        {
            Ships = new List<ShipData>();
        }

    }
}
