namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageObjectPickupRequest:MessagePackSerializableObject
    {
        public int RequestingShipID { get; set; }
        public int ObjectID { get; set; }
        public PickupableTypes ObjectType { get; set; }
    }

    /// <summary>
    /// Retarded enum name, too tired to care
    /// </summary>
    public enum PickupableTypes
    {
        DefensiveMine,
        Turret,
        FloatyAreaObject,
    }
}
