namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageWarpRequest:MessagePackSerializableObject
    {
        public int WarpholeIndex { get; set; }

        public int WarpingShipID { get; set; }

        public int DestinationAreaID { get; set; }
    }
}
