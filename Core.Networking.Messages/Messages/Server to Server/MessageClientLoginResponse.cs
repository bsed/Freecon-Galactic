using Core.Models.Enums;
using Freecon.Core.Networking.Models.ServerToServer;

namespace Freecon.Core.Networking.ServerToServer
{
    public class MessageRedisLoginDataResponse : MessageServerToServer
    {       
        public byte[] ServerIP { get; set; }
        public int ServerPort { get; set; }

        public int AccountID { get; set; }

        public LoginResult Result { get; set; }

        public MessageRedisLoginDataResponse()
        {
        }

        public MessageRedisLoginDataResponse(byte[] serverIP, int serverPort, int accountID, LoginResult result)
        {
            ServerIP = serverIP;
            ServerPort = serverPort;
            AccountID = accountID;
            Result = result;
        }


       
    }

}
