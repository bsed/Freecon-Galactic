namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageDockRequest:MessagePackSerializableObject
    {
        public int DockingShipID { get; set; }

        public int PortID { get; set; }

    }
}
