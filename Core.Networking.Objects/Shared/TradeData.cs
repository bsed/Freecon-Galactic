using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Networking.Objects
{
    public class ShipShipTradeData
    {
        public TradeData ShipA { get; set; }
        public TradeData ShipB { get; set; }
    }
    
    public class TradeData
    {
        /// <summary>
        /// Cargo being offered for trade
        /// </summary>
        public CargoData CargoData { get; set; }
        public int ShipID { get; set; }
        public bool Accepted { get; set; }

    }
}
