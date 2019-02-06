
namespace Freecon.Core.Networking.Models.Messages
{
    /// <summary>
    /// Message which for which information is communicated entirely by the MessageType.
    /// Fields are optional for simulator
    /// </summary>
    public class MessageEmptyMessage:MessagePackSerializableObject
    {
        public int? TargetAreaID;

        public int? TargetShipID;

        public float? Data;
    }
}
