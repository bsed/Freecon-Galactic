namespace Freecon.Core.Networking.Models.Objects
{
    /// <summary>
    /// Sends player information the client
    /// </summary>
    public class PlayerInfo
    {
        public int PlayerID { get; set; }

        /// <summary>
        /// The ship the client is currently controlling
        /// </summary>
        public int? ActiveShipID { get; set; }//Leaving the option for multiple ships

        public PlayerInfo()
        {
            
        }

        public PlayerInfo(int playerID, int? activeShipID)
        {
            PlayerID = playerID;
            ActiveShipID = activeShipID;
        }

    }
}
