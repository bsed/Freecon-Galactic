using Freecon.Core.Networking.Models.ServerToServer;
namespace Freecon.Core.Networking.ServerToServer
{

    public class MessageClientHandoff : MessageServerToServer
    {
        public int AccountID;
        public int ShipID;
        public int DestinationAreaID;
        public byte[] IPAddress;
        public int? ServerGameStateId;
        

        public MessageClientHandoff()
        {
        }

        public MessageClientHandoff(int accountID, int shipID, int destinationAreaID, byte[] iPAddress, int? serverGameStateId = null)
        { AccountID = accountID; ShipID = shipID; DestinationAreaID = destinationAreaID; IPAddress = iPAddress; ServerGameStateId = serverGameStateId; }


    }
    


}
