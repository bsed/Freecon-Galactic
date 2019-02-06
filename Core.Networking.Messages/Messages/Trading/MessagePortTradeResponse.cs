using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Networking.Models.Messages
{
    /// <summary>
    /// TODO: Ensure that on failure, the client polls the server for a port state UI update
    /// </summary>
    public class MessagePortTradeResponse:MessagePackSerializableObject
    {
        /// <summary>
        /// Updates the ship cargo
        /// </summary>
        public CargoData ShipCargo { get; set; }
        
        public string ResponseMessage { get; set; }


    }

    
}
