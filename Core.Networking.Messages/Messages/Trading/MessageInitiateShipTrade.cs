using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Core.Networking.Messages
{
    public class MessageInitiateShipTrade:MessagePackSerializableObject
    {
        /// <summary>
        /// Send the ship a cargo update, just in case
        /// </summary>
        public CargoData CurrentCargoData { get; set; }

        public string TargetUsername { get; set; }
    }
}
