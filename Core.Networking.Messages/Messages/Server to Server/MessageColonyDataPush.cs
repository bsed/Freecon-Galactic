using Freecon.Core.Networking.Models.ServerToServer;

namespace Freecon.Core.Networking.ServerToServer
{
    //Very lightweight, general purpose class
    public class MessageColonyDataPush : MessageServerToServer
    {
        public int ShipID { get; set; }//ShipID of the ship that is on the colony. 
        //This way we don't worry as much about spoofing. The ship must be on the colony to make changes.
        //The colony is found by searching for the corresponding ship's CurrentAreaID.

        public UpdateTypes UpdateType { get; set; }

        /// <summary>
        /// Use this to identify which objects an update applies to; E.G. slider number, factory ID
        /// </summary>
        public int FirstIdentifier { get; set; }

        /// <summary>
        /// This one's pushing it...if we start to rely on many commands with multiple arguments, I'll break this class up.
        /// </summary>
        public int SecondIdentifier { get; set; }

        /// <summary>
        /// Use this to pass value, as needed. E.G. slider percentage, resources to pick up
        /// </summary>
        public float Data { get; set; }

        public MessageColonyDataPush()
        {
        }
        
    }

    public enum UpdateTypes : byte
    {
        SetSlider,
        DropResource,
        WithdrawResource,
        AddConstructableToQueue,
        RemoveConstructableFromQueue,
    }
   
}
