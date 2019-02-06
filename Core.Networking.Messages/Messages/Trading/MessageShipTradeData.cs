using Freecon.Core.Networking.Objects;
using Freecon.Models.UI;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageShipTradeData:MessagePackSerializableObject
    {
        /// <summary>
        /// Most up to date trade data, predigested for UI display.
        /// </summary>
        public UIDisplayData TradeUIData { get; set; }

        public ShipShipTradeData TradeData { get; set; }

    }

    
}
