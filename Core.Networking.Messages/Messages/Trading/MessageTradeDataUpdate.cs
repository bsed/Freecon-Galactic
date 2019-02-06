using Freecon.Core.Networking.Objects;

namespace Freecon.Core.Networking.Models.Messages
{
    /// <summary>
    /// 1. Populate this message with the current trade data from the UI
    /// 2. Add the requested changes to this message
    /// 3. Send the message and do not update the UI until an update response is recieved. Server will first validate requested changes.
    /// </summary>
    public class MessageTradeUpdateData:MessagePackSerializableObject
    {
        public TradeData TradeData { get; set; }

    }
}
