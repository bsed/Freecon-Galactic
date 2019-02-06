namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageLandRequest:MessagePackSerializableObject
    {
        public int LandingShipID { get; set; }

        /// <summary>
        /// The area attempted to be landed on
        /// </summary>
        public int AreaID { get; set; }
    }
}
