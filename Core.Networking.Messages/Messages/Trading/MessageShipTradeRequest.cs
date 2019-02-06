namespace Freecon.Core.Networking.Models.Messages
{
    /// <summary>
    /// Use this to both initiate and respond affirmatively to trade requests.
    /// </summary>
    public class MessageShipTradeRequest:MessagePackSerializableObject
    {
        public string TargetPlayerUsername { get; set; }

    }
}
