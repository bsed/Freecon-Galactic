namespace Freecon.Client.Core.Objects
{

    /// <summary>
    /// Used for displaying ships currently docked in port
    /// </summary>
    public class PortShip
    {
        private string playerName;
        private int shipID;

        public PortShip(int shipID, string playerName)
        {
            this.shipID = shipID;
            this.playerName = playerName;
        }
    }
}
